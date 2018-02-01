using Pelco.Media.RTSP.SDP;
using System;

namespace Pelco.Media.RTSP.Client
{
    /// <summary>
    /// An IRtspInvoker is a request builder and invoker used to simplify
    /// creating and sending requests to an rtsp server.
    /// </summary>
    public interface IRtspInvoker
    {
        /// <summary>
        /// Sets the Request Uri to use. The Uri must be from the same host and port
        /// that the client connected to otherwise the request will not work.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IRtspInvoker Uri(Uri uri);

        /// <summary>
        /// Sets the SESSION RTSP header value.
        /// </summary>
        /// <param name="session">The rtsp session id</param>
        /// <returns></returns>
        IRtspInvoker Session(string session);

        /// <summary>
        /// Sets the TRANSPORT RTSP header value.
        /// </summary>
        /// <param name="transport">The transport to set</param>
        /// <returns></returns>
        IRtspInvoker Transport(TransportHeader transport);

        /// <summary>
        /// Adds an RTSP header to the request.
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="value">The value of the header</param>
        /// <returns></returns>
        IRtspInvoker AddHeader(string name, string value);

        #region Synchronous Methods
        
        /// <summary>
        /// Sends an RTSP OPTIONS call to an RTSP server.
        /// </summary>
        /// <returns><see cref="RtspResponse"/></returns>
        /// <exception cref="TimeoutException">If timed out waiting for the request</exception>
        /// <exception cref="RtspClientException">If an communication failure occurs.</exception>
        RtspResponse Options();

        /// <summary>
        /// Sends an RTSP DESCRIBE call to an RTSP server.
        /// </summary>
        /// <returns><see cref="RtspResponse"/></returns>
        /// <exception cref="TimeoutException">If timed out waiting for the request</exception>
        /// <exception cref="RtspClientException">If an communication failure occurs.</exception>
        RtspResponse Describe();

        SessionDescription GetSessionDescription();

        /// <summary>
        /// Sends an RTSP SETUP call to an RTSP server.
        /// </summary>
        /// <returns><see cref="RtspResponse"/></returns>
        /// <exception cref="TimeoutException">If timed out waiting for the request</exception>
        /// <exception cref="RtspClientException">If an communication failure occurs.</exception>
        RtspResponse SetUp();

        /// <summary>
        /// Sends an RTSP PLAY call to an RTSP server.
        /// </summary>
        /// <returns><see cref="RtspResponse"/></returns>
        /// <exception cref="TimeoutException">If timed out waiting for the request</exception>
        /// <exception cref="RtspClientException">If an communication failure occurs.</exception>
        RtspResponse Play();

        /// <summary>
        /// Sends an RTSP TEARDOWN call to an RTSP server.
        /// </summary>
        /// <returns><see cref="RtspResponse"/></returns>
        /// <exception cref="TimeoutException">If timed out waiting for the request</exception>
        /// <exception cref="RtspClientException">If an communication failure occurs.</exception>
        RtspResponse TearDown();

        /// <summary>
        /// Sends an RTSP GET_PARAMETER call to an RTSP server.
        /// </summary>
        /// <returns><see cref="RtspResponse"/></returns>
        /// <exception cref="TimeoutException">If timed out waiting for the request</exception>
        /// <exception cref="RtspClientException">If an communication failure occurs.</exception>
        RtspResponse GetParameter();

        #endregion

        #region Async Methods

        /// <summary>
        /// Asynchronously sends an RTSP OPTIONS call to an RTSP server.  If a response
        /// is received the call back will be invoked.
        /// </summary>
        /// <param name="callback">Callback for handling the server response</param>
        void OptionsAsync(RtspResponseCallback callback);

        /// <summary>
        /// Asynchronously sends an RTSP DESCRIBE call to an RTSP server.  If a response
        /// is received the call back will be invoked.
        /// </summary>
        /// <param name="callback">Callback for handling the server response</param>
        void DescribeAsync(RtspResponseCallback callback);

        /// <summary>
        /// Asynchronously sends an RTSP SETUP call to an RTSP server.  If a response
        /// is received the call back will be invoked.
        /// </summary>
        /// <param name="callback">Callback for handling the server response</param>
        void SetUpAsync(RtspResponseCallback callback);

        /// <summary>
        /// Asynchronously sends an RTSP PLAY call to an RTSP server.  If a response
        /// is received the call back will be invoked.
        /// </summary>
        /// <param name="callback">Callback for handling the server response</param>
        void PlayAsync(RtspResponseCallback callback);

        /// <summary>
        /// Asynchronously sends an RTSP TEARDOWN call to an RTSP server.  If a response
        /// is received the call back will be invoked.
        /// </summary>
        /// <param name="callback">Callback for handling the server response</param>
        void TeardownAsync(RtspResponseCallback callback);

        /// <summary>
        /// Asynchronously sends an RTSP GET_PARAMETER call to an RTSP server.  If a response
        /// is received the call back will be invoked.
        /// </summary>
        /// <param name="callback">Callback for handling the server response</param>
        void GetParameterAsync(RtspResponseCallback callback);

        #endregion
    }
}
