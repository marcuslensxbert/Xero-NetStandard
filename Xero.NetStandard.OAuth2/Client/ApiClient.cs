/* 
 * Xero Accounting API
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * Contact: api@xero.com
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using RestSharp;
using RestSharp.Serializers;
using RestSharpMethod = RestSharp.Method;
using RestSharp.Serializers.Xml;
using System.Threading;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("Xero.NetStandard.OAuth2.Test")]
namespace Xero.NetStandard.OAuth2.Client
{
    /// <summary>
    /// Allows RestSharp to Serialize/Deserialize JSON using our custom logic, but only when ContentType is JSON. 
    /// </summary>
    internal class CustomJsonCodec : IRestSerializer, ISerializer, IDeserializer
    {
        private readonly IReadableConfiguration _configuration;
        private ContentType _contentType = "application/json";
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            // OpenAPI generated types generally hide default constructors.
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new DefaultContractResolver
              {
                  NamingStrategy = new DefaultNamingStrategy()
                  {
                      OverrideSpecifiedNames = true
                  }
              },
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            CheckAdditionalContent = false,
            Culture = CultureInfo.InvariantCulture,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK",
        	DateParseHandling = DateParseHandling.DateTime,
            DefaultValueHandling = DefaultValueHandling.Include,
        	FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Double,
            Formatting = Formatting.None,
            MaxDepth = null,
            MetadataPropertyHandling = MetadataPropertyHandling.Default,
            MissingMemberHandling = MissingMemberHandling.Ignore,
        	NullValueHandling = NullValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
        	StringEscapeHandling = StringEscapeHandling.Default,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.None
        };

        public CustomJsonCodec(IReadableConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Serialize(object obj)
        {
            String result = JsonConvert.SerializeObject(obj, _serializerSettings);
            
            // when dealing with AU Payroll & Accounting API, we should convert DateTime object to Wcf Json Date String
            if (obj.GetType().FullName.Contains(@"Xero.NetStandard.OAuth2.Model.Accounting") || obj.GetType().FullName.Contains(@"Xero.NetStandard.OAuth2.Model.PayrollAu") )
            {
              string dateTimePattern = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z";

              Regex rgx = new Regex(dateTimePattern);
              Match match = rgx.Match(result);
              
              if (match.Success) {
                int indexAdjust = 0;
                foreach (Match m in rgx.Matches(result))
                {
                  DateTime dateTime = DateTime.Parse(m.Value);
                  DateTimeOffset dateTimeOffset   = new DateTimeOffset(dateTime);
                  var unixDateTime = dateTimeOffset.ToUniversalTime().ToUnixTimeMilliseconds();
                  var wcfJsonDateString = "/Date(" + unixDateTime + "+0000)/";
                  result = result.Remove(m.Index + indexAdjust, m.Length).Insert(m.Index + indexAdjust, wcfJsonDateString);
                  indexAdjust = indexAdjust + (wcfJsonDateString.Length - m.Value.Length);
                }
              }

              return result;
            };
            
            return result;
        }

        public T Deserialize<T>(RestResponse response)
        {
            var result = (T) Deserialize(response, typeof(T));
            return result;
        }

        /// <summary>
        /// Deserialize the JSON string into a proper object.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="type">Object type.</param>
        /// <returns>Object representation of the JSON string.</returns>
        internal object Deserialize(RestResponse response, Type type)
        {
            IReadOnlyCollection<HeaderParameter> headers = response.Headers;
            if (type == typeof(byte[])) // return byte array
            {
                return response.RawBytes;
            }

            // TODO: ? if (type.IsAssignableFrom(typeof(Stream)))
            if (type == typeof(Stream))
            {
                if (headers != null)
                {
                    var filePath = String.IsNullOrEmpty(_configuration.TempFolderPath)
                        ? Path.GetTempPath()
                        : _configuration.TempFolderPath;
                    var regex = new Regex(@"Content-Disposition=.*filename=['""]?([^'""\s]+)['""]?$");
                    foreach (var header in headers)
                    {
                        var match = regex.Match(header.ToString());
                        if (match.Success)
                        {
                            string fileName = filePath + ClientUtils.SanitizeFilename(match.Groups[1].Value.Replace("\"", "").Replace("'", ""));
                            File.WriteAllBytes(fileName, response.RawBytes);
                            return new FileStream(fileName, FileMode.Open);
                        }
                    }
                }
                var stream = new MemoryStream(response.RawBytes);
                return stream;
            }

            if (type.Name.StartsWith("System.Nullable`1[[System.DateTime")) // return a datetime object
            {
                return DateTime.Parse(response.Content, null, System.Globalization.DateTimeStyles.RoundtripKind);
            }

            if (type == typeof(String) || type.Name.StartsWith("System.Nullable")) // return primitive type
            {
                return ClientUtils.ConvertType(response.Content, type);
            }

            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApiException((int)response.StatusCode, response.Content);
            }

            // at this point, it must be a model (json)
            try
            {
                return JsonConvert.DeserializeObject(response.Content, type, _serializerSettings);
            }
            catch (Exception e)
            {
                throw new ApiException(500, e.Message);
            }
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }

        public ContentType ContentType
        {
            get { return _contentType; }
            set { throw new InvalidOperationException("Not allowed to set content type."); }
        }
        
        public string Serialize(Parameter parameter)
        {
            return Serialize(parameter.Value);
        }

        public ISerializer Serializer => this;

        public IDeserializer Deserializer => this;

        public string[] AcceptedContentTypes => new[]
        {
            "application/json",
            "text/json",
            "text/x-json",
            "text/javascript",
            "*+json",
            "*"
        };

        public SupportsContentType SupportsContentType => type => AcceptedContentTypes.Contains(type.Value);

        public DataFormat DataFormat => DataFormat.Json;
    }

    /// <summary>
    /// Override class used to support deserialising null enum values
    /// </summary>
    public class CustomStringEnumConverter : StringEnumConverter
    {
        /// <summary>
        /// Reads the JSON representation of the object.
        /// Has a special case where null is read into a 0-value enum.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType.IsEnum && reader.Value == null)
            {
                return Activator.CreateInstance(objectType);
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }

    /// <summary>
    /// Provides a default implementation of an Api client (both synchronous and asynchronous implementations),
    /// encapsulating general REST accessor use cases.
    /// </summary>
    public partial class ApiClient : ISynchronousClient, IAsynchronousClient
    {
        private readonly String _baseUrl;

        /// <summary>
        /// Allows for extending request processing for <see cref="ApiClient"/> generated code.
        /// </summary>
        /// <param name="request">The RestSharp request object</param>
        protected virtual void InterceptRequest(RestRequest request)
        {

        }

        /// <summary>
        /// Allows for extending response processing for <see cref="ApiClient"/> generated code.
        /// </summary>
        /// <param name="request">The RestSharp request object</param>
        /// <param name="response">The RestSharp response object</param>
        protected virtual void InterceptResponse(RestRequest request, RestResponse response)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClient" />, defaulting to the global configurations' base url.
        /// </summary>
        public ApiClient()
        {
            _baseUrl = Xero.NetStandard.OAuth2.Client.GlobalConfiguration.Instance.BasePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClient" />
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        public ApiClient(String basePath)
        {
           if (String.IsNullOrEmpty(basePath))
                throw new ArgumentException("basePath cannot be empty");

            _baseUrl = basePath;
        }

        /// <summary>
        /// Constructs the RestSharp version of an http method
        /// </summary>
        /// <param name="method">Swagger Client Custom HttpMethod</param>
        /// <returns>RestSharp's HttpMethod instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private RestSharpMethod Method(HttpMethod method)
        {
            RestSharpMethod other;
            switch (method)
            {
                case HttpMethod.Get:
                    other = RestSharpMethod.Get;
                    break;
                case HttpMethod.Post:
                    other = RestSharpMethod.Post;
                    break;
                case HttpMethod.Put:
                    other = RestSharpMethod.Put;
                    break;
                case HttpMethod.Delete:
                    other = RestSharpMethod.Delete;
                    break;
                case HttpMethod.Head:
                    other = RestSharpMethod.Head;
                    break;
                case HttpMethod.Options:
                    other = RestSharpMethod.Options;
                    break;
                case HttpMethod.Patch:
                    other = RestSharpMethod.Patch;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("method", method, null);
            }

            return other;
        }

        /// <summary>
        /// Provides all logic for constructing a new RestSharp <see cref="RestRequest"/>.
        /// At this point, all information for querying the service is known. Here, it is simply
        /// mapped into the RestSharp request.
        /// </summary>
        /// <param name="method">The http verb.</param>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <returns>[private] A new RestRequest instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private RestRequest newRequest(
            HttpMethod method,
            String path,
            RequestOptions options,
            IReadableConfiguration configuration)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (options == null) throw new ArgumentNullException("options");
            if (configuration == null) throw new ArgumentNullException("configuration");
            
            RestRequest request = new RestRequest(path, Method(method))
            {
                Timeout = configuration.Timeout
            };

            if (options.PathParameters != null)
            {
                foreach (var pathParam in options.PathParameters)
                {
                    request.AddParameter(pathParam.Key, pathParam.Value, ParameterType.UrlSegment);
                }
            }
            
            if (options.QueryParameters != null)
            {
                foreach (var queryParam in options.QueryParameters)
                {
                    foreach (var value in queryParam.Value)
                    {
                        request.AddQueryParameter(queryParam.Key, value);
                    }
                }
            }

            if (options.HeaderParameters != null)
            {
                foreach (var headerParam in options.HeaderParameters)
                {
                    foreach (var value in headerParam.Value)
                    {
                        request.AddHeader(headerParam.Key, value);
                    }
                }
            }

            if (options.FormParameters != null)
            {
                foreach (var formParam in options.FormParameters)
                {
                    request.AddParameter(formParam.Key, formParam.Value);
                }
            }

            if (options.Data != null)
            {
                if (options.HeaderParameters != null)
                {
                    var contentTypes = options.HeaderParameters["Content-Type"];
                    if (contentTypes == null || contentTypes.Any(header => header.Contains("application/json")))
                    {
                        request.RequestFormat = DataFormat.Json;
                    }
                    else
                    {
                        // TODO: Generated client user should add additional handlers. RestSharp only supports XML and JSON, with XML as default.
                    }
                }
                else
                {
                    // Here, we'll assume JSON APIs are more common. XML can be forced by adding produces/consumes to openapi spec explicitly.
                    request.RequestFormat = DataFormat.Json;
                }

                if (options.PathParameters.ContainsKey("FileName"))
                {
                    //restsharp needs the bytestream to go here. addfile adds metadata which corrupts file. 
                    request.AddParameter("application/octet-stream",(byte[])options.Data,ParameterType.RequestBody);
                }
                else
                {
                    request.AddJsonBody(options.Data);
                }            
            }

            if (options.FileParameters != null)
            {
                foreach (var fileParam in options.FileParameters)
                {
                    var memStream = fileParam.Value as MemoryStream;
                    var fileStream = fileParam.Value as FileStream;
                    if (fileStream != null)
                    {
                        var bytes = ClientUtils.ReadAsBytes(fileParam.Value);
                        var name = System.IO.Path.GetFileName(fileStream.Name);
                        options.FormParameters.TryGetValue("mimeType", out var contentType);


                        request.AddFile(name, bytes, name, contentType);
                    }
                    else if (memStream != null)
                    {
                        var bytes = memStream.ToArray();
                        var hasName = options.FormParameters.TryGetValue("filename", out var name);
                        options.FormParameters.TryGetValue("mimeType", out var contentType);
                        
                        if (!hasName)
                        {
                            throw new ApiException(400, "Files API: No filename provided when uploading file");
                        }
                        request.AddFile(name, bytes, name, contentType);
                    }
                    else
                    {
                        var bytes = ClientUtils.ReadAsBytes(fileParam.Value);
                        var hasName = options.FormParameters.TryGetValue("filename", out var name);
                        options.FormParameters.TryGetValue("mimeType", out var contentType);

                        if (!hasName)
                        {
                            throw new ApiException(400, "Files API: No filename provided when uploading file");
                        }
                        request.AddFile(name, bytes, name, contentType);
                    }
                }
            }

            return request;
        }

        private ApiResponse<T> toApiResponse<T>(RestResponse<T> response)
        {
            T result = response.Data;
            var transformed = new ApiResponse<T>(response.StatusCode, new Multimap<string, string>(), result)
            {
                ErrorText = response.ErrorMessage,
                Cookies = new List<Cookie>(),
                Content = response.Content
            };
            
            if (response.Headers != null)
            {
                foreach (var responseHeader in response.Headers)
                {
                    transformed.Headers.Add(responseHeader.Name, ClientUtils.ParameterToString(responseHeader.Value));
                }
            }

            if (response.Cookies != null)
            {
                for(int i = 0; i < response.Cookies.Count; i++)
                {
                    Cookie responseCookies = response.Cookies[i];
                    transformed.Cookies.Add(
                        new Cookie(
                            responseCookies.Name, 
                            responseCookies.Value, 
                            responseCookies.Path, 
                            responseCookies.Domain)
                        );
                }
            }

            return transformed;
        }

        private async Task<ApiResponse<T>> Exec<T>(RestRequest req, IReadableConfiguration configuration, CancellationToken cancellationToken = default)
        {
            RestClientOptions clientOptions = new RestClientOptions(_baseUrl)
            {
                MaxTimeout = configuration.Timeout
            };
            if (configuration.UserAgent != null)
            {
                clientOptions.UserAgent = configuration.UserAgent;
            }

            RestClient client = new RestClient(
                clientOptions,
                configureSerialization: _ => _.UseSerializer(() =>
                {
                    var serializer = new CustomJsonCodec(configuration);
                    return serializer;
                })
                    .UseSerializer<XmlRestSerializer>()
            );

            if (configuration.Cookies != null && configuration.Cookies.Count > 0)
            {
                foreach (var cookie in configuration.Cookies)
                {
                    req.CookieContainer.Add(new Cookie(cookie.Name, cookie.Value));
                }
            }

            InterceptRequest(req);
            var response = await client.ExecuteAsync<T>(req, cancellationToken);
            InterceptResponse(req, response);

            var result = toApiResponse(response);
            if (response.ErrorMessage != null)
            {
                result.ErrorText = response.ErrorMessage;
            }

            if (response.Cookies != null && response.Cookies.Count > 0)
            {
                if(result.Cookies == null) result.Cookies = new List<Cookie>();
                for (int i = 0; i < response.Cookies.Count; i++)
                {
                    var restResponseCookie = response.Cookies[i];
                    var cookie = new Cookie(
                        restResponseCookie.Name,
                        restResponseCookie.Value,
                        restResponseCookie.Path,
                        restResponseCookie.Domain
                    )
                    {
                        Comment = restResponseCookie.Comment,
                        CommentUri = restResponseCookie.CommentUri,
                        Discard = restResponseCookie.Discard,
                        Expired = restResponseCookie.Expired,
                        Expires = restResponseCookie.Expires,
                        HttpOnly = restResponseCookie.HttpOnly,
                        Port = restResponseCookie.Port,
                        Secure = restResponseCookie.Secure,
                        Version = restResponseCookie.Version
                    };
                    
                    result.Cookies.Add(cookie);
                }
            }
            return result;
        }
        
        #region IAsynchronousClient
        /// <summary>
        /// Make a HTTP GET request (async).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public async Task<ApiResponse<T>> GetAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            var config = configuration ?? GlobalConfiguration.Instance;
            return await Exec<T>(newRequest(HttpMethod.Get, path, options, config), config, cancellationToken);
        }

        /// <summary>
        /// Make a HTTP POST request (async).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public async Task<ApiResponse<T>> PostAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            var config = configuration ?? GlobalConfiguration.Instance;
            return await Exec<T>(newRequest(HttpMethod.Post, path, options, config), config, cancellationToken);
        }

        /// <summary>
        /// Make a HTTP PUT request (async).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public async Task<ApiResponse<T>> PutAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            var config = configuration ?? GlobalConfiguration.Instance;
            return await Exec<T>(newRequest(HttpMethod.Put, path, options, config), config, cancellationToken);
        }

        /// <summary>
        /// Make a HTTP DELETE request (async).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public async Task<ApiResponse<T>> DeleteAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            var config = configuration ?? GlobalConfiguration.Instance;
            return await Exec<T>(newRequest(HttpMethod.Delete, path, options, config), config, cancellationToken);
        }

        /// <summary>
        /// Make a HTTP HEAD request (async).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public async Task<ApiResponse<T>> HeadAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            var config = configuration ?? GlobalConfiguration.Instance;
            return await Exec<T>(newRequest(HttpMethod.Head, path, options, config), config, cancellationToken);
        }

        /// <summary>
        /// Make a HTTP OPTION request (async).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public async Task<ApiResponse<T>> OptionsAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            var config = configuration ?? GlobalConfiguration.Instance;
            return await Exec<T>(newRequest(HttpMethod.Options, path, options, config), config, cancellationToken);
        }

        /// <summary>
        /// Make a HTTP PATCH request (async).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public async Task<ApiResponse<T>> PatchAsync<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            var config = configuration ?? GlobalConfiguration.Instance;
            return await Exec<T>(newRequest(HttpMethod.Patch, path, options, config), config, cancellationToken);
        }
        #endregion IAsynchronousClient

        #region ISynchronousClient
        /// <summary>
        /// Make a HTTP GET request (synchronous).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public ApiResponse<T> Get<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            return GetAsync<T>(path, options, configuration, cancellationToken).Result;
        }

        /// <summary>
        /// Make a HTTP POST request (synchronous).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public ApiResponse<T> Post<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            return PostAsync<T>(path, options, configuration, cancellationToken).Result;
        }

        /// <summary>
        /// Make a HTTP PUT request (synchronous).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public ApiResponse<T> Put<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            return PutAsync<T>(path, options, configuration, cancellationToken).Result;
        }

        /// <summary>
        /// Make a HTTP DELETE request (synchronous).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public ApiResponse<T> Delete<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            return DeleteAsync<T>(path, options, configuration, cancellationToken).Result;
        }

        /// <summary>
        /// Make a HTTP HEAD request (synchronous).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public ApiResponse<T> Head<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            return HeadAsync<T>(path, options, configuration, cancellationToken).Result;
        }

        /// <summary>
        /// Make a HTTP OPTION request (synchronous).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public ApiResponse<T> Options<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            return OptionsAsync<T>(path, options, configuration, cancellationToken).Result;
        }

        /// <summary>
        /// Make a HTTP PATCH request (synchronous).
        /// </summary>
        /// <param name="path">The target path (or resource).</param>
        /// <param name="options">The additional request options.</param>
        /// <param name="configuration">A per-request configuration object. It is assumed that any merge with
        /// GlobalConfiguration has been done before calling this method.</param>
        /// <param name="cancellationToken">Cancellation token enables cancellation between threads. Defaults to CancellationToken.None</param>
        /// <returns>A Task containing ApiResponse</returns>
        public ApiResponse<T> Patch<T>(string path, RequestOptions options, IReadableConfiguration configuration = null, CancellationToken cancellationToken = default)
        {
            return PatchAsync<T>(path, options, configuration, cancellationToken).Result;
        }
        #endregion ISynchronousClient
    }
}
