using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace MercadoPago
{
    public class MercadoPagoClient
    {
        public static readonly String version = "0.3.3";

        private readonly String client_id = null;
        private readonly String client_secret = null;
        private readonly String ll_access_token = null;
        private Hashtable access_data = null;
        private bool sandbox = false;

        /**
         * Instantiate MP with credentials
         * @param client_id
         * @param client_secret
         * @return An access_token to use with the API
         */
        public MercadoPagoClient(String client_id, String client_secret)
        {
            this.client_id = client_id;
            this.client_secret = client_secret;
        }

        /**
         * Instantiate MP with Long Live Access Token
         * @param client_id
         * @param client_secret
         * @return An access_token to use with the API
         */
        public MercadoPagoClient(String ll_access_token)
        {
            this.ll_access_token = ll_access_token;
        }

        public bool sandboxMode()
        {
            return this.sandbox;
        }

        public bool sandboxMode(bool enable)
        {
            this.sandbox = enable;

            return this.sandbox;
        }

        /**
         * Get Access Token for API use
         */
        public String getAccessToken()
        {
            if (this.ll_access_token != null)
            {
                return this.ll_access_token;
            }

            Dictionary<String, String> appClientValues = new Dictionary<String, String>();
            appClientValues.Add("grant_type", "client_credentials");
            appClientValues.Add("client_id", this.client_id);
            appClientValues.Add("client_secret", this.client_secret);

            String appClientValuesQuery = this.buildQuery(appClientValues);

            Hashtable access_data = RestClient.post("/oauth/token", appClientValuesQuery, RestClient.MIME_FORM);

            if (((int) access_data["status"]) == 200)
            {
                this.access_data = (Hashtable) access_data["response"];
                return (String) this.access_data["access_token"];
            }
            else
            {
                throw new Exception(JSON.JsonEncode(access_data));
            }
        }

        /**
         * Get information for specific payment
         * @param id
         * @return
         */
        public Hashtable getPayment(String id)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            String uriPrefix = this.sandbox ? "/sandbox" : "";

            Hashtable paymentInfo = RestClient.get(uriPrefix + "/collections/notifications/" + id + "?access_token=" + accessToken);

            return paymentInfo;
        }

        public Hashtable getPaymentInfo(String id)
        {
            return this.getPayment(id);
        }

        /**
         * Get information for specific authorized payment
         * @param id
         * @return
         */
        public Hashtable getAuthorizedPayment(String id)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            Hashtable authorizedPaymentInfo = RestClient.get("/authorized_payments/" + id + "?access_token=" + accessToken);

            return authorizedPaymentInfo;
        }

        /**
         * Refund accredited payment
         * @param id
         * @return
         */
        public Hashtable refundPayment(String id)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            Hashtable refundStatus = new Hashtable();
            refundStatus["status"] = "refunded";

            Hashtable response = RestClient.put("/collections/" + id + "?access_token=" + accessToken, refundStatus);

            return response;
        }

        /**
         * Cancel pending payment
         * @param id
         * @return
         */
        public Hashtable cancelPayment(String id)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            Hashtable cancelStatus = new Hashtable();
            cancelStatus["status"] = "cancelled";

            Hashtable response = RestClient.put("/collections/" + id + "?access_token=" + accessToken, cancelStatus);

            return response;
        }

        /**
         * Cancel preapproval payment
         * @param id
         * @return
         */
        public Hashtable cancelPreapprovalPayment(String id)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            Hashtable cancelStatus = new Hashtable();
            cancelStatus["status"] = "cancelled";

            Hashtable response = RestClient.put("/preapproval/" + id + "?access_token=" + accessToken, cancelStatus);

            return response;
        }

        /**
         * Search payments according to filters, with pagination
         * @param filters
         * @param offset
         * @param limit
         * @return
         */
        public Hashtable searchPayment(Dictionary<String, String> filters, long offset = 0, long limit = 0)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            filters.Add("offset", offset.ToString());
            filters.Add("limit", limit.ToString());

            String filtersQuery = this.buildQuery(filters);

            String uriPrefix = this.sandbox ? "/sandbox" : "";

            Hashtable collectionResult = RestClient.get(uriPrefix + "/collections/search?" + filtersQuery + "&access_token=" + accessToken);
            return collectionResult;
        }

        /**
         * Create a checkout preference
         * @param preference
         * @return
         */
        public Hashtable createPreference(String preference)
        {
            Hashtable preferenceJSON = (Hashtable) JSON.JsonDecode(preference);
            return this.createPreference(preferenceJSON);
        }

        public Hashtable createPreference(Hashtable preference)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            Hashtable preferenceResult = RestClient.post("/checkout/preferences?access_token=" + accessToken, preference);
            return preferenceResult;
        }

        /**
         * Update a checkout preference
         * @param string $id
         * @param array $preference
         * @return
         */
        public Hashtable updatePreference(String id, String preference)
        {
            Hashtable preferenceJSON = (Hashtable) JSON.JsonDecode(preference);
            return this.updatePreference(id, preferenceJSON);
        }

        public Hashtable updatePreference(String id, Hashtable preference)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            Hashtable preferenceResult = RestClient.put("/checkout/preferences/" + id + "?access_token=" + accessToken, preference);
            return preferenceResult;
        }

        /**
         * Get a checkout preference
         * @param id
         * @return
         */
        public Hashtable getPreference(String id)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            Hashtable preferenceResult = RestClient.get("/checkout/preferences/" + id + "?access_token=" + accessToken);
            return preferenceResult;
        }

        /**
         * Create a preapproval payment
         * @param preference
         * @return
         */
        public Hashtable createPreapprovalPayment(String preapprovalPayment)
        {
            Hashtable preapprovalPaymentJSON = (Hashtable) JSON.JsonDecode(preapprovalPayment);
            return this.createPreapprovalPayment(preapprovalPaymentJSON);
        }

        public Hashtable createPreapprovalPayment(Hashtable preapprovalPayment)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            Hashtable preapprovalPaymentResult = RestClient.post("/preapproval?access_token=" + accessToken, preapprovalPayment);
            return preapprovalPaymentResult;
        }

        /**
         * Get a preapproval payment
         * @param id
         * @return
         */
        public Hashtable getPreapprovalPayment(String id)
        {
            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            Hashtable preapprovalPaymentResult = RestClient.get("/preapproval/" + id + "?access_token=" + accessToken);
            return preapprovalPaymentResult;
        }

        /**
         * Generic resource get
         * @param uri
         * @param parameters
         * @param authenticate
         * @return
         */
        public Hashtable get(String uri, Dictionary<String, String> parameters, bool authenticate)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<String, String>();
            }
            if (authenticate)
            {
                String accessToken;
                try
                {
                    accessToken = this.getAccessToken();
                }
                catch (Exception e)
                {
                    return (Hashtable) JSON.JsonDecode(e.Message);
                }

                parameters.Add("access_token", accessToken);
            }

            if (parameters.Count > 0)
            {
                uri += (uri.Contains("?") ? "&" : "?") + this.buildQuery(parameters);
            }

            Hashtable result = RestClient.get(uri);
            return result;
        }

        /**
         * Generic resource get
         * @param uri
         * @param authenticate
         * @return
         */
        public Hashtable get(String uri, bool authenticate)
        {
            return this.get(uri, null, authenticate);
        }

        /**
         * Generic resource get
         * @param uri
         * @param parameters
         * @return
         */
        public Hashtable get(String uri, Dictionary<String, String> parameters)
        {
            return this.get(uri, parameters, true);
        }

        /**
         * Generic resource post
         * @param uri
         * @param data
         * @return
         */
        public Hashtable post(String uri, String data)
        {
            return this.post(uri, data, null);
        }

        public Hashtable post(String uri, String data, Dictionary<String, String> parameters)
        {
            Hashtable dataJSON = (Hashtable) JSON.JsonDecode(data);
            return this.post(uri, dataJSON, parameters);
        }

        public Hashtable post(String uri, Hashtable data)
        {
            return this.post(uri, data, null);
        }

        public Hashtable post(String uri, Hashtable data, Dictionary<String, String> parameters)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<String, String>();
            }

            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            parameters.Add("access_token", accessToken);

            uri += (uri.Contains("?") ? "&" : "?") + this.buildQuery(parameters);

            Hashtable result = RestClient.post(uri, data);
            return result;
        }

        /**
         * Generic resource put
         * @param uri
         * @param data
         * @return
         */
        public Hashtable put(String uri, String data)
        {
            return this.put(uri, data, null);
        }

        public Hashtable put(String uri, String data, Dictionary<String, String> parameters)
        {
            Hashtable dataJSON = (Hashtable) JSON.JsonDecode(data);
            return this.put(uri, dataJSON, parameters);
        }

        public Hashtable put(String uri, Hashtable data)
        {
            return this.put(uri, data, null);
        }

        public Hashtable put(String uri, Hashtable data, Dictionary<String, String> parameters)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<String, String>();
            }

            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            parameters.Add("access_token", accessToken);

            uri += (uri.Contains("?") ? "&" : "?") + this.buildQuery(parameters);

            Hashtable result = RestClient.put(uri, data);
            return result;
        }

        /**
         * Generic resource delete
         * @param uri
         * @param parameters
         * @return
         */
        public Hashtable delete(String uri)
        {
            return this.delete(uri, null);
        }

        public Hashtable delete(String uri, Dictionary<String, String> parameters)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<String, String>();
            }

            String accessToken;
            try
            {
                accessToken = this.getAccessToken();
            }
            catch (Exception e)
            {
                return (Hashtable) JSON.JsonDecode(e.Message);
            }

            parameters.Add("access_token", accessToken);

            uri += (uri.Contains("?") ? "&" : "?") + this.buildQuery(parameters);

            Hashtable result = RestClient.delete(uri);
            return result;
        }

        /*****************************************************************************************************/
        private String buildQuery<T>(Dictionary<String, T> parameters)
        {
            String[] query = new String[parameters.Count];
            int index = 0;

            var enumerator = parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                String val = enumerator.Current.Value != null ? enumerator.Current.Value.ToString() : "";
                val = WebUtility.UrlEncode(val);
                query[index++] = enumerator.Current.Key + "=" + val;
            }
            return String.Join("&", query);
        }

        private static class Util
        {
            public static T get<K, T>(Dictionary<K, T> dict, K key, T def)
            {
                return dict.ContainsKey(key) ? dict[key] : def;
            }
        }

        private static class RestClient
        {
            private const String API_BASE_URL = "https://api.mercadopago.com";
            public const String MIME_JSON = "application/json";
            public const String MIME_FORM = "application/x-www-form-urlencoded";

            private static Hashtable exec(String method, String uri, Object data, String contentType)
            {
                Hashtable response;

                var request = (HttpWebRequest) WebRequest.Create(API_BASE_URL + uri);

//                request.UserAgent = "MercadoPago .NET SDK v" + MP.version;
                request.Accept = MIME_JSON;
                request.Method = method;
                request.ContentType = contentType;
                setData(request, data, contentType);

                String responseBody = null;
                try
                {
                    var apiResult = (HttpWebResponse) request.GetResponseAsync().Result;
                    responseBody = new StreamReader(apiResult.GetResponseStream()).ReadToEnd();

                    response = new Hashtable();
                    response["status"] = (int) apiResult.StatusCode;
                    response["response"] = JSON.JsonDecode(responseBody);
                }
                catch (WebException e)
                {
                    Console.WriteLine(e.Message);
                    responseBody = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                    try
                    {
                        response = new Hashtable();
                        response["status"] = (int) ((HttpWebResponse) e.Response).StatusCode;
                        response["response"] = JSON.JsonDecode(responseBody);
                    }
                    catch (Exception e2)
                    {
                        response = new Hashtable();
                        response["status"] = 500;
                        response["response"] = e2.Message;
                    }
                }

                return response;
            }

            private static void setData(HttpWebRequest request, Object data, String contentType)
            {
                if (data != null)
                {
                    String dataString;
                    if (data is Hashtable)
                        dataString = JSON.JsonEncode(data);
                    else
                        dataString = data.ToString();
                    if (dataString.Length > 0)
                    {
                        using (Stream requestStream = request.GetRequestStreamAsync().Result)
                        {
                            using (StreamWriter writer = new StreamWriter(requestStream))
                            {
                                writer.Write(dataString);
                            }
                        }
                    }
                }
            }

            public static Hashtable get(String uri, String contentType = MIME_JSON)
            {
                return exec("GET", uri, null, contentType);
            }

            public static Hashtable post(String uri, Object data, String contentType = MIME_JSON)
            {
                return exec("POST", uri, data, contentType);
            }

            public static Hashtable put(String uri, Object data, String contentType = MIME_JSON)
            {
                return exec("PUT", uri, data, contentType);
            }

            public static Hashtable delete(String uri, String contentType = MIME_JSON)
            {
                return exec("DELETE", uri, null, contentType);
            }
        }
    }


    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    ///
    /// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
    /// All numbers are parsed to doubles.
    /// </summary>
    public class JSON
    {
        public const int TOKEN_NONE = 0;
        public const int TOKEN_CURLY_OPEN = 1;
        public const int TOKEN_CURLY_CLOSE = 2;
        public const int TOKEN_SQUARED_OPEN = 3;
        public const int TOKEN_SQUARED_CLOSE = 4;
        public const int TOKEN_COLON = 5;
        public const int TOKEN_COMMA = 6;
        public const int TOKEN_STRING = 7;
        public const int TOKEN_NUMBER = 8;
        public const int TOKEN_TRUE = 9;
        public const int TOKEN_FALSE = 10;
        public const int TOKEN_NULL = 11;

        private const int BUILDER_CAPACITY = 2000;

        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static object JsonDecode(string json)
        {
            bool success = true;

            return JsonDecode(json, ref success);
        }

        /// <summary>
        /// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <param name="success">Successful parse?</param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static object JsonDecode(string json, ref bool success)
        {
            success = true;
            if (json != null)
            {
                char[] charArray = json.ToCharArray();
                int index = 0;
                object value = ParseValue(charArray, ref index, ref success);
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a Hashtable / ArrayList object into a JSON string
        /// </summary>
        /// <param name="json">A Hashtable / ArrayList</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string JsonEncode(object json)
        {
            StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
            bool success = SerializeValue(json, builder);
            return (success ? builder.ToString() : null);
        }

        protected static Hashtable ParseObject(char[] json, ref int index, ref bool success)
        {
            Hashtable table = new Hashtable();
            int token;

            // {
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                token = LookAhead(json, index);
                if (token == JSON.TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == JSON.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == JSON.TOKEN_CURLY_CLOSE)
                {
                    NextToken(json, ref index);
                    return table;
                }
                else
                {
                    // name
                    string name = ParseString(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    // :
                    token = NextToken(json, ref index);
                    if (token != JSON.TOKEN_COLON)
                    {
                        success = false;
                        return null;
                    }

                    // value
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    table[name] = value;
                }
            }

            return table;
        }

        protected static ArrayList ParseArray(char[] json, ref int index, ref bool success)
        {
            ArrayList array = new ArrayList();

            // [
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                int token = LookAhead(json, index);
                if (token == JSON.TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == JSON.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == JSON.TOKEN_SQUARED_CLOSE)
                {
                    NextToken(json, ref index);
                    break;
                }
                else
                {
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                    {
                        return null;
                    }

                    array.Add(value);
                }
            }

            return array;
        }

        protected static object ParseValue(char[] json, ref int index, ref bool success)
        {
            switch (LookAhead(json, index))
            {
                case JSON.TOKEN_STRING:
                    return ParseString(json, ref index, ref success);
                case JSON.TOKEN_NUMBER:
                    return ParseNumber(json, ref index, ref success);
                case JSON.TOKEN_CURLY_OPEN:
                    return ParseObject(json, ref index, ref success);
                case JSON.TOKEN_SQUARED_OPEN:
                    return ParseArray(json, ref index, ref success);
                case JSON.TOKEN_TRUE:
                    NextToken(json, ref index);
                    return true;
                case JSON.TOKEN_FALSE:
                    NextToken(json, ref index);
                    return false;
                case JSON.TOKEN_NULL:
                    NextToken(json, ref index);
                    return null;
                case JSON.TOKEN_NONE:
                    break;
            }

            success = false;
            return null;
        }

        protected static string ParseString(char[] json, ref int index, ref bool success)
        {
            StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
            char c;

            EatWhitespace(json, ref index);

            // "
            c = json[index++];

            bool complete = false;
            while (!complete)
            {
                if (index == json.Length)
                {
                    break;
                }

                c = json[index++];
                if (c == '"')
                {
                    complete = true;
                    break;
                }
                else if (c == '\\')
                {
                    if (index == json.Length)
                    {
                        break;
                    }
                    c = json[index++];
                    if (c == '"')
                    {
                        s.Append('"');
                    }
                    else if (c == '\\')
                    {
                        s.Append('\\');
                    }
                    else if (c == '/')
                    {
                        s.Append('/');
                    }
                    else if (c == 'b')
                    {
                        s.Append('\b');
                    }
                    else if (c == 'f')
                    {
                        s.Append('\f');
                    }
                    else if (c == 'n')
                    {
                        s.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        s.Append('\r');
                    }
                    else if (c == 't')
                    {
                        s.Append('\t');
                    }
                    else if (c == 'u')
                    {
                        int remainingLength = json.Length - index;
                        if (remainingLength >= 4)
                        {
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint;
                            if (!(success = UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)))
                            {
                                return "";
                            }
                            // convert the integer codepoint to a unicode char and add to string
                            s.Append(Char.ConvertFromUtf32((int) codePoint));
                            // skip 4 chars
                            index += 4;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    s.Append(c);
                }
            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return s.ToString();
        }

        protected static double ParseNumber(char[] json, ref int index, ref bool success)
        {
            EatWhitespace(json, ref index);

            int lastIndex = GetLastIndexOfNumber(json, index);
            int charLength = (lastIndex - index) + 1;

            double number;
            success = Double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

            index = lastIndex + 1;
            return number;
        }

        protected static int GetLastIndexOfNumber(char[] json, int index)
        {
            int lastIndex;

            for (lastIndex = index; lastIndex < json.Length; lastIndex++)
            {
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
                {
                    break;
                }
            }
            return lastIndex - 1;
        }

        protected static void EatWhitespace(char[] json, ref int index)
        {
            for (; index < json.Length; index++)
            {
                if (" \t\n\r".IndexOf(json[index]) == -1)
                {
                    break;
                }
            }
        }

        protected static int LookAhead(char[] json, int index)
        {
            int saveIndex = index;
            return NextToken(json, ref saveIndex);
        }

        protected static int NextToken(char[] json, ref int index)
        {
            EatWhitespace(json, ref index);

            if (index == json.Length)
            {
                return JSON.TOKEN_NONE;
            }

            char c = json[index];
            index++;
            switch (c)
            {
                case '{':
                    return JSON.TOKEN_CURLY_OPEN;
                case '}':
                    return JSON.TOKEN_CURLY_CLOSE;
                case '[':
                    return JSON.TOKEN_SQUARED_OPEN;
                case ']':
                    return JSON.TOKEN_SQUARED_CLOSE;
                case ',':
                    return JSON.TOKEN_COMMA;
                case '"':
                    return JSON.TOKEN_STRING;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return JSON.TOKEN_NUMBER;
                case ':':
                    return JSON.TOKEN_COLON;
            }
            index--;

            int remainingLength = json.Length - index;

            // false
            if (remainingLength >= 5)
            {
                if (json[index] == 'f' &&
                    json[index + 1] == 'a' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 's' &&
                    json[index + 4] == 'e')
                {
                    index += 5;
                    return JSON.TOKEN_FALSE;
                }
            }

            // true
            if (remainingLength >= 4)
            {
                if (json[index] == 't' &&
                    json[index + 1] == 'r' &&
                    json[index + 2] == 'u' &&
                    json[index + 3] == 'e')
                {
                    index += 4;
                    return JSON.TOKEN_TRUE;
                }
            }

            // null
            if (remainingLength >= 4)
            {
                if (json[index] == 'n' &&
                    json[index + 1] == 'u' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 'l')
                {
                    index += 4;
                    return JSON.TOKEN_NULL;
                }
            }

            return JSON.TOKEN_NONE;
        }

        protected static bool SerializeValue(object value, StringBuilder builder)
        {
            bool success = true;

            if (value is string)
            {
                success = SerializeString((string) value, builder);
            }
            else if (value is Hashtable)
            {
                success = SerializeObject((Hashtable) value, builder);
            }
            else if (value is ArrayList)
            {
                success = SerializeArray((ArrayList) value, builder);
            }
            else if ((value is Boolean) && ((Boolean) value == true))
            {
                builder.Append("true");
            }
            else if ((value is Boolean) && ((Boolean) value == false))
            {
                builder.Append("false");
            }
            else if (value is ValueType)
            {
                // thanks to ritchie for pointing out ValueType to me
                success = SerializeNumber(Convert.ToDouble(value), builder);
            }
            else if (value == null)
            {
                builder.Append("null");
            }
            else
            {
                success = false;
            }
            return success;
        }

        protected static bool SerializeObject(Hashtable anObject, StringBuilder builder)
        {
            builder.Append("{");

            IDictionaryEnumerator e = anObject.GetEnumerator();
            bool first = true;
            while (e.MoveNext())
            {
                string key = e.Key.ToString();
                object value = e.Value;

                if (!first)
                {
                    builder.Append(", ");
                }

                SerializeString(key, builder);
                builder.Append(":");
                if (!SerializeValue(value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("}");
            return true;
        }

        protected static bool SerializeArray(ArrayList anArray, StringBuilder builder)
        {
            builder.Append("[");

            bool first = true;
            for (int i = 0; i < anArray.Count; i++)
            {
                object value = anArray[i];

                if (!first)
                {
                    builder.Append(", ");
                }

                if (!SerializeValue(value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("]");
            return true;
        }

        protected static bool SerializeString(string aString, StringBuilder builder)
        {
            builder.Append("\"");

            char[] charArray = aString.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (c == '"')
                {
                    builder.Append("\\\"");
                }
                else if (c == '\\')
                {
                    builder.Append("\\\\");
                }
                else if (c == '\b')
                {
                    builder.Append("\\b");
                }
                else if (c == '\f')
                {
                    builder.Append("\\f");
                }
                else if (c == '\n')
                {
                    builder.Append("\\n");
                }
                else if (c == '\r')
                {
                    builder.Append("\\r");
                }
                else if (c == '\t')
                {
                    builder.Append("\\t");
                }
                else
                {
                    int codepoint = Convert.ToInt32(c);
                    if ((codepoint >= 32) && (codepoint <= 126))
                    {
                        builder.Append(c);
                    }
                    else
                    {
                        builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                    }
                }
            }

            builder.Append("\"");
            return true;
        }

        protected static bool SerializeNumber(double number, StringBuilder builder)
        {
            builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
            return true;
        }
    }
}