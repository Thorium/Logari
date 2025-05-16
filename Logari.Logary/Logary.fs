/// This has Logary dependency that is registering the Logary as Microsoft.Extensions.Logging.Abstraction
[<AutoOpen>]
module Logging

open Microsoft.Extensions.Logging
open Logary

/// Old Logary direct calls:
let logger = lazy(
#if LOGARY5
        Logary.Log.create "default"
#else
        Logary.Logging.getCurrentLogger()
#endif
    )

/// General Logary logger
let writeLog x =
    let l = logger.Force()
    Logary.Logger.logSimple l x

type LogaryLogger(name) =
    new() = LogaryLogger "default"
    
    interface ILogger with

        member __.IsEnabled(_) = true
        member __.BeginScope(_) = { new System.IDisposable with
                                        member this.Dispose() = ()}
        
        member __.Log(level, evetId:EventId, state, ex, formatter) =
            let msg = unbox<Logari.CustomMessage>(state)
            let mutable lmsg =
                match level with
                | Microsoft.Extensions.Logging.LogLevel.Debug ->
                    Logary.Message.eventDebug msg.Message
                | Microsoft.Extensions.Logging.LogLevel.Information ->
                    Logary.Message.eventInfo msg.Message
                | Microsoft.Extensions.Logging.LogLevel.Warning ->
                    Logary.Message.eventWarn msg.Message
                | Microsoft.Extensions.Logging.LogLevel.Error ->
                    Logary.Message.eventError msg.Message
                | Microsoft.Extensions.Logging.LogLevel.Critical ->
                    Logary.Message.eventFatal msg.Message
                | Microsoft.Extensions.Logging.LogLevel.Trace
                | Microsoft.Extensions.Logging.LogLevel.None
                | _ -> Logary.Message.eventVerbose msg.Message

            for i in msg.Fields do
                lmsg <-
#if LOGARY5
                    lmsg |> Logary.Message.setField i.Key i.Value
#else
                    lmsg |> Logary.Message.setFieldFromObject i.Key i.Value
#endif
                ()
            writeLog lmsg

[<ProviderAlias("LogaryLogger")>]
type LogaryLoggerProvider() =
    let loggers = System.Collections.Concurrent.ConcurrentDictionary<string, LogaryLogger>()
    interface ILoggerProvider with
        member _.CreateLogger categoryName =
            loggers.GetOrAdd(categoryName, fun name -> new LogaryLogger(name));
        member _.Dispose() = loggers.Clear()

let mutable loggingSetup = false

/// To avoid lazy-loading issues, it's better to also call this manually when your program starts.
let setupLogging() =
    if not loggingSetup then
        Logari.logger <- lazy(LogaryLogger() :> ILogger)
    loggingSetup <- true
setupLogging()
