using System;
using System.Collections.Immutable;

namespace Pelco.PDK.Media.Pipeline
{
    public sealed class Pipeline
    {
        private ImmutableList<ISink> _sinks;
        private ImmutableList<ISource> _sources;
        private ImmutableList<ITransform> _pipeline;

        private Pipeline(ImmutableList<ISource> sources, ImmutableList<ISink> sinks, ImmutableList<ITransform> pipeline)
        {
            _sinks = sinks;
            _sources = sources;
            _pipeline = pipeline;
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
            _sources.ForEach((src) =>
            {
                src.Start();
            });

            _pipeline.ForEach((transform) =>
            {
                transform.Start();
            });
        }

        /// <summary>
        /// Stops a pipeline.
        /// </summary>
        public void Stop()
        {
            _sources.ForEach((src) => Stop(src));
            _pipeline.ForEach((trans) => Stop((ISource)trans));
            _sinks.ForEach((sink) => Stop(sink));
        }

        #region Private methods

        private void Stop(ISink sink)
        {
            try
            {
                sink.Stop();
            }
            catch (Exception e)
            {

            }
        }

        private void Stop(ISource source)
        {
            try
            {
                source.Stop();
            }
            catch (Exception e)
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
                this.source = transform;

                return this;
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
                this.source = null;

                return this;
            }

            public Pipeline Build()
            {
                return new Pipeline(srcBuilder.ToImmutable(), sinkBuilder.ToImmutable(), transformBuilder.ToImmutable()); 
            }
        }
    }
}
