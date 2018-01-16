 using System;
using System.Collections.Immutable;

namespace Pelco.Media.Pipeline
{
    public sealed class MediaPipeline
    {
        private ImmutableList<ISink> _sinks;
        private ImmutableList<ISource> _sources;
        private ImmutableList<ITransform> _transforms;

        private MediaPipeline(ImmutableList<ISource> sources, ImmutableList<ISink> sinks, ImmutableList<ITransform> pipeline)
        {
            _sinks = sinks;
            _sources = sources;
            _transforms = pipeline;
        }

        /// <summary>
        /// Creates a new Pipeline.Builder instance.
        /// </summary>
        /// <returns></returns>
        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        /// <summary>
        /// Starts a pipeline.
        /// </summary>
        public void Start()
        {
            _sources.ForEach(src => src.Start());
            _transforms.ForEach(trans => trans.Start());
        }

        /// <summary>
        /// Stops a pipeline.
        /// </summary>
        public void Stop()
        {
            _sources.ForEach(src => Stop(src));
            _transforms.ForEach(trans => Stop((ISource)trans));
            _sinks.ForEach(sink => Stop(sink));
        }
        
        /// <summary>
        /// Sets the flushing flag on the Media sources.
        /// </summary>
        /// <param name="flushing"></param>
        public void SetFlushing(bool flushing)
        {
            _sources.ForEach(src => src.Flushing = flushing);
            _transforms.ForEach(trans => trans.Flushing = flushing);
        }

        #region Private methods

        private void Stop(ISink sink)
        {
            try
            {
                sink.Stop();
            }
            catch (Exception)
            {

            }
        }

        private void Stop(ISource source)
        {
            try
            {
                source.Stop();
            }
            catch (Exception)
            {

            }
        }

        #endregion

        public sealed class Builder
        {
            private ISource source;
            private ImmutableList<ISink>.Builder sinkBuilder;
            private ImmutableList<ISource>.Builder srcBuilder;
            private ImmutableList<ITransform>.Builder transformBuilder;

            public Builder()
            {
                sinkBuilder = ImmutableList.CreateBuilder<ISink>();
                srcBuilder = ImmutableList.CreateBuilder<ISource>();
                transformBuilder = ImmutableList.CreateBuilder<ITransform>();
            }

            public Builder Source(ISource source)
            {
                if (source == null)
                {
                    throw new ArgumentNullException("Cannot add null source");
                }
                else if (this.source != null)
                {
                    throw new ArgumentException("Source has already been set.");
                }

                srcBuilder.Add(source);

                this.source = source;

                return this;
            }

            public Builder SourceIf(bool condition, ISource source)
            {
                return condition ? Source(source) : this;
            }

            public Builder SourceIf(Func<bool> func, ISource source)
            {
                return func.Invoke() ? Source(source) : this;
            }

            public Builder Transform(ITransform transform)
            {
                if (transform == null)
                {
                    throw new ArgumentNullException("Cannot add null transform");
                }
                else if (this.source == null)
                {
                    throw new ArgumentException("Must first set a source");
                }

                transformBuilder.Add(transform);
                this.source.DownstreamLink = transform;
                ((ISource)transform).UpstreamLink = this.source;
                this.source = transform;

                return this;
            }

            public Builder TransformIf(bool condition, ITransform transform)
            {
                return condition ? Transform(transform) : this;
            }

            public Builder TransformIf(Func<bool> func, ITransform transform)
            {
                return func.Invoke() ? Transform(transform) : this;
            }

            public Builder Sink(ISink sink)
            {
                if (sink == null)
                {
                    throw new ArgumentNullException("Cannot add null sink");
                }
                else if (this.source == null)
                {
                    throw new ArgumentException("Must first set a source");
                }

                sinkBuilder.Add(sink);
                this.source.DownstreamLink = sink;
                sink.UpstreamLink = this.source;
                this.source = null;

                return this;
            }

            public Builder SinkIf(bool condition, ISink sink)
            {
                return condition ? Sink(sink) : this;
            }

            public Builder SinkIf(Func<bool> func, ISink sink)
            {
                return func.Invoke() ? Sink(sink) : this;
            }

            public MediaPipeline Build()
            {
                return new MediaPipeline(srcBuilder.ToImmutable(), sinkBuilder.ToImmutable(), transformBuilder.ToImmutable()); 
            }
        }
    }
}
