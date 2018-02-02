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
    /// Base class representing an pipeline upstream event.
    /// </summary>
    public class MediaEvent
    {
        /// <summary>
        /// The event's topic.
        /// </summary>
        public string Topic { get; set; }
    }
}
