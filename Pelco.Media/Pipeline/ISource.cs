//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
namespace Pelco.Media.Pipeline
{
    /// <summary>
    /// Pipeline interface for pushing data into a pipeline (downstream)
    /// </summary>
    public interface ISource
    {
        /// <summary>
        /// Sets the downstream pipeline link.  This method can be used to
        /// link multiple pipeline instances together.
        /// </summary>
        ISink DownstreamLink { set; }

        /// <summary>
        /// Sets the upstream pipline link.  The upstream link is used to pass
        /// events downstream.
        /// </summary>
        ISource UpstreamLink { set; }

        /// <summary>
        /// Gets and sets the flushing flag.  This flag is set when a pipeline state
        /// change occurs.  This allows sources to purge data from the previous state.
        /// </summary>
        bool Flushing { get; set; }

        /// <summary>
        /// Starts the pipeline source.  Usually called before the pipeline data starts flowing.
        /// </summary>
        void Start();

        /// <summary>
        /// Called before the pipeline destruction occurs.  Can be used to clean up resources.
        /// </summary>
        void Stop();

        /// <summary>
        /// Called when a media event is received.  A media event is an event sent upstream by
        /// a transform or sink
        /// </summary>
        /// <param name="e"></param>
        void OnMediaEvent(MediaEvent e);
    }
}
