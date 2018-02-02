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
    /// Transform for converting an object type into a <see cref="ByteBuffer"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ObjectTypeToBufferTransformBase<T> : TransformBase, IObjectTypeSink<T>
    {
        /// <summary>
        /// <see cref="IObjectTypeSink{T}.HandleObject(T)"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract bool HandleObject(T obj);
    }
}
