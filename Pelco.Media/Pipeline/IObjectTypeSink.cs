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
    /// A sink the consumes a object instead of a buffer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectTypeSink<T>: ISink
    {
        bool HandleObject(T obj);
    }
}
