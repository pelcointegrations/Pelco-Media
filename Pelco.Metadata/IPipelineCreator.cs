using Pelco.PDK.Media.Pipeline;

namespace Pelco.PDK.Metadata
{
    /// <summary>
    /// Used by the <see cref="VxMetadataPlayer"/> to build a metadata processing pipeline.
    /// </summary>
    public interface IPipelineCreator
    {
        /// <summary>
        /// To use a <see cref="VxMetadataPlayer"/> an implementer must create a user defined <see cref="MediaPipeline"/>
        /// to process the metadata, using transforms, and display it to the user by defining a view sink.
        /// 
        /// Example of creating a pipeline.
        /// <code>
        /// protected MediaPipeline CreatePipeline(ISource src)
        /// {
        ///     return MediaPipeline.CreateBuilder().Source(src)
        ///                                         .TransForm(new MyMetadataTransform())
        ///                                         .Sink(new MyMetadataViewerSink())
        ///                                         .Build();
        /// }
        /// </code>
        /// </summary>
        /// <param name="src">The Pipeline's source for metadata.</param>
        /// <returns>A <see cref="MediaPipeline"/> for processing a metadata stream</returns>
        MediaPipeline CreatePipeline(ISource src);
    }
}
