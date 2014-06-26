﻿/*
 * Copyright 2014 Splunk, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"): you may
 * not use this file except in compliance with the License. You may obtain
 * a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

namespace Splunk.ModularInputs
{
    using System;
using System.Xml.Serialization;

    /// <summary>
    /// The <see cref="SingleValueParameter"/> class represents a parameter
    /// that contains a single value.
    /// </summary>
    [XmlRoot("param")]
    public class SingleValueParameter : Parameter
    {
        /// <summary>
        /// The value of the parameter.
        /// </summary>
        /// <remarks>
        /// This value is used for XML serialization and deserialization.
        /// <example>Sample XML</example>
        /// <code>
        /// <param name="param1">value1</param>
        /// </code>
        /// </remarks>
        [XmlText]
        public string Value { get; set; }

        public string ToString()
        {
            return this.Value;
        }

        public int ToInt()
        {
            return int.Parse(this.Value);
        }

        public double ToDouble()
        {
            return double.Parse(this.Value);
        }

        public float ToFloat()
        {
            return float.Parse(this.Value);
        }

        public long ToLong()
        {
            return long.Parse(this.Value);
        }

        public bool ToBool()
        {
            return Util.ParseSplunkBoolean(this.Value);
        }
    }
}
