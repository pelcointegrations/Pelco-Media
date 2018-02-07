//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;

namespace Pelco.Media.Metadata
{
    [Serializable]
    public class PlayerInitializationException : Exception
    {
        public PlayerInitializationException() : base()
        {

        }

        public PlayerInitializationException(string msg) : base(msg)
        {

        }
    }
}
