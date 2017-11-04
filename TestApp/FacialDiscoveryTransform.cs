using FacialDetectionCommon;
using NLog;
using Pelco.Media.Pipeline;
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace FacialRecognition.Metadata
{
    public class FacialDiscoveryTransform : BufferToObjectTypeTransformBase<FacialDiscovery>
    {
        private readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private XmlSerializer _serializer;

        public FacialDiscoveryTransform()
        {
            _serializer = new XmlSerializer(typeof(FacialDiscovery));
        }

        public override bool WriteBuffer(ByteBuffer buffer)
        {
            try
            {
                var xml = buffer.ToString(Encoding.UTF8);
                LOG.Info(xml);
                using (var reader = new StringReader(xml))
                {
                    return PushObject((FacialDiscovery)_serializer.Deserialize(reader));
                }
            }
            catch (Exception e)
            {
                LOG.Error($"Unable to process facial detection metadata, reason={e.Message}");
                return true;
            }
        }
    }
}
