//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
namespace Pelco.Media.Common
{
    /// <summary>
    /// Authentication credentials.
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Credentials()
        {

        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="username">Credentials username</param>
        /// <param name="password">Credentials password</param>
        public Credentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the password
        /// </summary>
        public string Password { get; }
    }
}
