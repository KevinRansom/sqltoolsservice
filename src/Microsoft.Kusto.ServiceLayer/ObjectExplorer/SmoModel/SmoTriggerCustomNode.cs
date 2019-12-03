﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Kusto.ServiceLayer.ObjectExplorer.Nodes;

namespace Microsoft.Kusto.ServiceLayer.ObjectExplorer.DataSourceModel
{
    /// <summary>
    /// Status for triggers
    /// </summary>
    internal partial class TriggersChildFactory : DataSourceChildFactoryBase
    {
        public static readonly Lazy<List<NodeSmoProperty>> SmoPropertiesLazy = new Lazy<List<NodeSmoProperty>>(() => new List<NodeSmoProperty>
        {
            new NodeSmoProperty
            {
                Name = "IsEnabled",
                ValidFor = ValidForFlag.All
            }
        });

        public override string GetNodeStatus(object objectMetadata, QueryContext oeContext)
        {
            return TriggersCustomeNodeHelper.GetStatus(objectMetadata);
        }

        public override IEnumerable<NodeSmoProperty> SmoProperties => SmoPropertiesLazy.Value;
    }

    internal partial class ServerLevelServerTriggersChildFactory : DataSourceChildFactoryBase
    {
        public override string GetNodeStatus(object objectMetadata, QueryContext oeContext)
        {
            return TriggersCustomeNodeHelper.GetStatus(objectMetadata);
        }

        public override IEnumerable<NodeSmoProperty> SmoProperties
        {
            get
            {
                return TriggersChildFactory.SmoPropertiesLazy.Value;
            }
        }
    }

    internal partial class DatabaseTriggersChildFactory : DataSourceChildFactoryBase
    {
        public override string GetNodeStatus(object objectMetadata, QueryContext oeContext)
        {
            return TriggersCustomeNodeHelper.GetStatus(objectMetadata);
        }

        public override IEnumerable<NodeSmoProperty> SmoProperties
        {
            get
            {
                return TriggersChildFactory.SmoPropertiesLazy.Value;
            }
        }
    }

    internal static class TriggersCustomeNodeHelper
    {
        internal static string GetStatus(object context)
        {
            Trigger trigger = context as Trigger;
            if (trigger != null)
            {
                if (!trigger.IsEnabled)
                {
                    return "Disabled";
                }
            }

            ServerDdlTrigger serverDdlTrigger = context as ServerDdlTrigger;
            if (serverDdlTrigger != null)
            {
                if (!serverDdlTrigger.IsEnabled)
                {
                    return "Disabled";
                }
            }

            DatabaseDdlTrigger databaseDdlTrigger = context as DatabaseDdlTrigger;
            if (databaseDdlTrigger != null)
            {
                if (!databaseDdlTrigger.IsEnabled)
                {
                    return "Disabled";
                }
            }

            return string.Empty;
        }
    }
}