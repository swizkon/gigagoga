using System;


namespace com.gigagoga.rest
{
    /// <summary>
    /// Enum containing all available http verbs in current version.
    /// </summary>
    [FlagsAttribute]
    public enum HttpVerbs : int
    {
        /// <summary>
        /// GET
        /// </summary>
        GET,
        /// <summary>
        /// POST
        /// </summary>
        POST,
        /// <summary>
        /// PUT
        /// </summary>
        PUT,
        /// <summary>
        /// DELETE
        /// </summary>
        DELETE,
        /// <summary>
        /// HEAD
        /// </summary>
        HEAD,
        /// <summary>
        /// PATCH
        /// </summary>
        PATCH,
        /// <summary>
        /// OPTIONS
        /// </summary>
        OPTIONS
    }
}