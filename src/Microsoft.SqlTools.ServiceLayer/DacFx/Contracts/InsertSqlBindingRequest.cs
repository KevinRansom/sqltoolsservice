﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
using System.Collections.Generic;
using Microsoft.SqlTools.Hosting.Protocol.Contracts;
using Microsoft.SqlTools.ServiceLayer.SchemaCompare.Contracts;
using Microsoft.SqlTools.ServiceLayer.Utility;

namespace Microsoft.SqlTools.ServiceLayer.DacFx.Contracts
{
    /// <summary>
    /// Parameters for inserting a sql binding
    /// </summary>
    public class InsertSqlBindingParams
    {
        /// <summary>
        /// Gets or sets the filePath
        /// </summary>
        public string filePath { get; set;}

        /// <summary>
        /// Gets or sets the function name
        /// </summary>
        public string functionName { get; set; }

        /// <summary>
        /// Gets or sets the object name
        /// </summary>
        public string objectName { get; set; }
    }

    /// <summary>
    /// Defines the Insert Sql Input Binding request
    /// </summary>
    class InsertSqlInputBindingRequest
    {
        public static readonly RequestType<InsertSqlBindingParams, ResultStatus> Type =
            RequestType<InsertSqlBindingParams, ResultStatus>.Create("dacfx/sqlInputBinding");

    }
    /// <summary>
    /// Defines the DacFx parse tsql request type
    /// </summary>
    class InsertSqlOutputBindingRequest
    {
        public static readonly RequestType<InsertSqlBindingParams, ResultStatus> Type =
            RequestType<InsertSqlBindingParams, ResultStatus>.Create("dacfx/sqlOutputBinding");
    }
}
