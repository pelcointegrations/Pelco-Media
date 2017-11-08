using System;
using System.Text.RegularExpressions;

namespace Pelco.Media.Common
{
    public class MimeType
    {
        public static readonly string APPLICATION_TYPE = "application";
        public static readonly string AUDIO_TYPE = "audio";
        public static readonly string IMAGE_TYPE = "image";
        public static readonly string TEXT_TYPE = "text";
        public static readonly string VIDEO_TYPE = "video";
        public static readonly string WILDCARD = "*";

        public static readonly MimeType ANY_TYPE = Create(WILDCARD, WILDCARD);

        // Application types
        public static readonly MimeType ANY_APPLICATION = CreateApplicationType(WILDCARD);
        public static readonly MimeType ONVIF_APPLICATION = CreateApplicationType("vnd.onvif.metadata");

        // Audio types
        public static readonly MimeType ANY_AUDIO = CreateAudioType(WILDCARD);
        public static readonly MimeType AAC_AUDIO = CreateAudioType("aac");
        public static readonly MimeType DVI4_AUDIO = CreateAudioType("DVI4");
        public static readonly MimeType GSM_AUDIO = CreateAudioType("GSM");
        public static readonly MimeType G722_AUDIO = CreateAudioType("G722");
        public static readonly MimeType G723_AUDIO = CreateAudioType("G723");
        public static readonly MimeType G726_16_AUDIO = CreateAudioType("G726-16");
        public static readonly MimeType G726_24_AUDIO = CreateAudioType("G726-24");
        public static readonly MimeType G726_32_AUDIO = CreateAudioType("G726-32");
        public static readonly MimeType G726_40_AUDIO = CreateAudioType("G726-40");
        public static readonly MimeType L16_AUDIO = CreateAudioType("L16");
        public static readonly MimeType MP4_AUDIO = CreateAudioType("mp4");
        public static readonly MimeType MPEG_AUDIO = CreateAudioType("mpeg");
        public static readonly MimeType PCMU_AUDIO = CreateAudioType("PCMU");
        public static readonly MimeType PCMA_AUDIO = CreateAudioType("PCMA");

        // Image types
        public static readonly MimeType ANY_IMAGE = CreateImageType(WILDCARD);

        // Text types
        public static readonly MimeType ANY_TEXT = CreateTextType(WILDCARD);

        // Video types
        public static readonly MimeType ANY_VIDEO = CreateVideoType(WILDCARD);
        public static readonly MimeType MPEG4_VIDEO = CreateVideoType("mp4v-es");
        public static readonly MimeType MJPEG_VIDEO = CreateVideoType("JPEG");
        public static readonly MimeType H264_VIDEO = CreateVideoType("h264");
        public static readonly MimeType H265_VIDEO = CreateVideoType("h265");
        public static readonly MimeType H261_VIDEO = CreateVideoType("H261");
        public static readonly MimeType H263_VIDEO = CreateVideoType("H263");

        public MimeType(string type, string subtype)
        {
            Type = type;
            Subtype = subtype;
        }

        public string Type { get; private set; }

        public string Subtype { get; private set; }

        public bool Is(MimeType type)
        {
            return (type.Type.Equals(WILDCARD) || type.Type.Equals(Type))
                   && (type.Subtype.Equals(WILDCARD) || type.Subtype.Equals(Subtype));
        }

        public static MimeType Parse(string value)
        {
            var parts = Regex.Split(value, @"\s*/\s*");
            if (parts.Length == 1)
            {
                return MimeType.Create(parts[0].Trim(), WILDCARD);
            }
            else if(parts.Length == 2)
            {
                var subType = string.IsNullOrEmpty(parts[1]) ? WILDCARD : parts[1].Trim();
                return MimeType.Create(parts[0].Trim(), subType);
            }

            throw new ArgumentException("Invalid mimetype provided.");
        }

        public static MimeType Create(string type, string subtype)
        {
            return new MimeType(type, subtype);
        }

        public static MimeType CreateApplicationType(string subtype)
        {
            return new MimeType(APPLICATION_TYPE, subtype);
        }

        public static MimeType CreateAudioType(string subtype)
        {
            return new MimeType(APPLICATION_TYPE, subtype);
        }

        public static MimeType CreateImageType(string subtype)
        {
            return new MimeType(IMAGE_TYPE, subtype);
        }

        public static MimeType CreateTextType(string subtype)
        {
            return new MimeType(TEXT_TYPE, subtype);
        }

        public static MimeType CreateVideoType(string subtype)
        {
            return new MimeType(VIDEO_TYPE, subtype);
        }
    }
}
