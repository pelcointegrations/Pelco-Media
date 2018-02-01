using NLog;
using Pelco.Media.RTP;
using System;

namespace Pelco.Media.Pipeline.Transforms
{
    /// <summary>
    /// Base RTP depacketizer class. Used to depacketize (rebuild frames) from RTP packets.
    /// </summary>
    public abstract class RtpDepacketizerBase : TransformBase
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private IRtpDemarcator _doesPacketEndThisFrame;
        private IRtpDemarcator _doesPacketBelongToNewFrame;

        public RtpDepacketizerBase(IRtpDemarcator belongsToNewFrame, IRtpDemarcator endsThisFrame)
        {
            _doesPacketEndThisFrame = endsThisFrame ?? throw new ArgumentNullException("endsThisFrame cannot be null");
            _doesPacketBelongToNewFrame = belongsToNewFrame ?? throw new ArgumentNullException("belontsToNewFrame cannot be null");
        }

        /// <summary>
        /// Flag indicating if the frame being processed is damaged. Meaning
        /// that it is missing RTP packets containing pieces of it's payload.
        /// </summary>
        protected bool IsDamaged { get; set; }

        /// <summary>
        /// <see cref="TransformBase.WriteBuffer(ByteBuffer)"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override bool WriteBuffer(ByteBuffer buffer)
        {
            try
            {
                var packet = RtpPacket.Decode(buffer);
                if (_doesPacketBelongToNewFrame.Check(packet))
                {
                    AssembleAndPush();
                }

                addRtpPacket(packet);

                if (_doesPacketEndThisFrame.Check(packet))
                {
                    AssembleAndPush();
                }
            }
            catch (Exception e)
            {
                LOG.Error(e, $"Failed to process RTP packet, reason: {e.Message}");
            }

            return true;
        }

        /// <summary>
        /// Called when a frame should be assembled. Meaning that the frame
        /// boundry has been detected from the RTP packets.
        /// </summary>
        /// <returns></returns>
        protected abstract ByteBuffer Assemble();

        /// <summary>
        /// Called to add the payload to the currently processing frame.
        /// </summary>
        /// <param name="packet"></param>
        protected abstract void addRtpPacket(RtpPacket packet);

        private void AssembleAndPush()
        {
            var frame = Assemble();
            if (frame.Length > 0)
            {
                frame.IsDamaged = IsDamaged;
                PushBuffer(frame);
            }
        }
    }

    /// <summary>
    /// Default implementation of the an RTP packetizer.
    /// </summary>
    public class DefaultRtpDepacketizer : RtpDepacketizerBase
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private ByteBuffer _frame;
        private bool _processingFragment;
        private ushort _expectedNextSeqNum;

        public DefaultRtpDepacketizer() : base (new TimestampDemarcator(), new MarkerDemarcator())
        {
            _frame = new ByteBuffer();
            _processingFragment = false;
            _expectedNextSeqNum = 0;
        }

        protected override void addRtpPacket(RtpPacket packet)
        {
            ushort seqNum = packet.SequenceNumber;

            if (_processingFragment)
            {
                if (_expectedNextSeqNum != seqNum)
                {
                    LOG.Debug($"Lost packet expected sequence number '{_expectedNextSeqNum}' got '{seqNum}'");
                    _processingFragment = false;
                    IsDamaged = true;
                }
                else
                {
                    _frame.Write(packet.Payload);
                }
            }
            else if (IsDamaged)
            {
                LOG.Debug($"Disgarding fragment '{seqNum}' from damaged frame");
            }
            else
            {
                _processingFragment = true;
                _frame.Write(packet.Payload);
            }

            _expectedNextSeqNum = ++seqNum;
        }

        protected override ByteBuffer Assemble()
        {
            var assembled = _frame;
            _frame = new ByteBuffer();
            _processingFragment = false;
            IsDamaged = false;

            assembled.MarkReadOnly();
            return assembled;
        }
    }
}
