﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace jnonce.WebApi.VersionedRouting
{
    /// <summary>
    /// Gets the Api version from an Http request header
    /// </summary>
    public class HttpHeaderApiVersionProvider : IApiVersionProvider
    {
        private string headerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaderApiVersionProvider"/> class.
        /// </summary>
        /// <param name="headerName">Name of the header.</param>
        public HttpHeaderApiVersionProvider(string headerName)
        {
            this.headerName = headerName;
        }

        /// <summary>
        /// Tries to get the Api version from the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="version">The version retrieved, if any.</param>
        /// <returns>
        /// true on success, false if no version was found.
        /// </returns>
        public bool TryGetApiVersion(HttpRequestMessage message, out int version)
        {
            IEnumerable<string> values;
            if (message.Headers.TryGetValues(headerName, out values))
            {
                var flattenedHeaders = values.Take(2).ToArray();
                if (flattenedHeaders.Length == 1)
                {
                    int apiVersion;
                    if (Int32.TryParse(flattenedHeaders[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out apiVersion)
                        && (apiVersion & 0xffffff) == apiVersion)
                    {
                        version = apiVersion;
                        return true;
                    }
                }
            }

            version = 0;
            return false;
        }
    }
}