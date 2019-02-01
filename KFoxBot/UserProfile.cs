// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace KFoxBot
{
    /// <summary>
    /// This is our application state. Just a regular serializable .NET class.
    /// </summary>
    public class UserProfile
    {
        public string Name { get; set; }

        public string ShoppingItem { get; set; }

        public string ShoppingProduct { get; set; }

        public string ShoppingMall { get; set; }
    }
}
