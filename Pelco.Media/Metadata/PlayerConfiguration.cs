//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Common;
using Pelco.Media.Metadata.Api;
using System;

namespace Pelco.Media.Metadata
{
    /// <summary>
    /// Configuration used to initialize a <see cref="VxMetadataPlayer"/>
    /// </summary>
    public class PlayerConfiguration
    {
        /// <summary>
        /// Uri of the RTSP endpoint to communicate with.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Authentication credenitals for authenticating against the RTSP server.
        /// If credentials are not required then this can be ommited or set to null.
        /// </summary>
        public Credentials Creds { get; set; }

        /// <summary>
        /// A mime type filter used to filter out media tracks that are not of interest.
        /// A filter type of null will result is using the default metadata filter mime
        /// type of application/*
        /// </summary>
        public MimeType TypeFilter { get; set; }

        /// <summary>
        /// A <see cref="PipelineCreator"/> used to create a metadata specific processing pipeline.
        /// </summary>
        public IPipelineCreator PipelineCreator { get; set; }
    }
}
