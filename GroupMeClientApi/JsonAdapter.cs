// Adapted from https://bytefish.de/blog/restsharp_custom_json_serializer/

using System.IO;
using Newtonsoft.Json;
using RestSharp.Deserializers;
using RestSharp.Serializers;

// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace GroupMeClientApi
{
    /// <summary>
    /// <see cref="JsonAdapter"/> provides a wrapper for <see cref="JsonSerializer"/> to match the <see cref="ISerializer"/> and <see cref="IDeserializer"/> interfaces.
    /// </summary>
    public class JsonAdapter : ISerializer, IDeserializer
    {
        private readonly Newtonsoft.Json.JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAdapter"/> class.
        /// </summary>
        /// <param name="serializer">The serializer that should be wrapped.</param>
        public JsonAdapter(Newtonsoft.Json.JsonSerializer serializer)
        {
            this.serializer = serializer;
        }

        /// <summary>
        /// Gets a default <see cref="JsonAdapter"/> for GroupMe operations.
        /// </summary>
        public static JsonAdapter Default => new JsonAdapter(new Newtonsoft.Json.JsonSerializer()
        {
            NullValueHandling = NullValueHandling.Ignore,
        });

        /// <inheritdoc/>
        public string ContentType
        {
            get { return "application/json"; } // Probably used for Serialization?
            set { }
        }

        /// <inheritdoc/>
        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    this.serializer.Serialize(jsonTextWriter, obj);

                    var result = stringWriter.ToString();
                    return result;
                }
            }
        }

        /// <inheritdoc/>
        public T Deserialize<T>(RestSharp.IRestResponse response)
        {
            var content = response.Content;

            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return this.serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }
}