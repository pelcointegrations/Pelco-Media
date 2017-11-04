using Pelco.Metadata;
using Pelco.Media.Pipeline;
using Pelco.Metadata.UI;
using FacialDetectionCommon;

namespace FacialRecognition.Metadata
{
    public class PipelineCreator : IPipelineCreator
    {
        private IObjectTypeSink<FacialDiscovery> _sink;
        
        public PipelineCreator(IVideoOverlayCanvas<FacialDiscovery> sink)
        {
            _sink = sink;
        }

        public MediaPipeline CreatePipeline(ISource src)
        {
            return MediaPipeline.CreateBuilder()
                                .Source(src)
                                .Transform(new FacialDiscoveryTransform())
                                .Sink(_sink)
                                .Build();
        }
    }
}
