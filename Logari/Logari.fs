/// Module to repalce Logary with Microsoft.Extensions.Logging
/// Usage: Replace Logary.Message.eventDebug "hi {a}" |> Logary.Message "a" "world" |> writeLogSimple
/// With:          Logari.Message.eventDebug "hi {a}" |> Logari.Message "a" "world" |> writeLogSimple
module Logari

let loggerFactory =
    Microsoft.Extensions.Logging.LoggerFactory.Create(fun builder ->
        // Cretae a temporary Microsoft.Extensions.Logging.Console
        let _ = Microsoft.Extensions.Logging.ConsoleLoggerExtensions.AddSimpleConsole builder
        ())

/// This is an instance of ILogger.
/// Initially some temp-logger, can be replaced.
/// Can be used as poor-man's DI/IoC.
let mutable logger = lazy(loggerFactory.CreateLogger("Temp-logger")) 

type CustomMessage =
    { Level: Microsoft.Extensions.Logging.LogLevel 
      Message: string
      Fields: System.Collections.Generic.Dictionary<string, obj>
    } with override this.ToString() =
            if this = Unchecked.defaultof<CustomMessage> then "" else
            let sb = System.Text.StringBuilder this.Message
            if this.Fields = null then sb.ToString()
            else
            for i in this.Fields do
                if i.Value <> null then
                    sb.Replace("{" + i.Key + "}", i.Value.ToString()) |> ignore
            sb.ToString()

module Message =
    let eventDebug (msg:string) = {Level = Microsoft.Extensions.Logging.LogLevel.Debug; Message = msg; Fields = new System.Collections.Generic.Dictionary<_,_>()}
    let eventInfo (msg:string) = {Level = Microsoft.Extensions.Logging.LogLevel.Information; Message = msg; Fields = new System.Collections.Generic.Dictionary<_,_>()}
    let eventWarn (msg:string) = {Level = Microsoft.Extensions.Logging.LogLevel.Warning; Message = msg; Fields = new System.Collections.Generic.Dictionary<_,_>()}
    let eventError (msg:string) = {Level = Microsoft.Extensions.Logging.LogLevel.Error; Message = msg; Fields = new System.Collections.Generic.Dictionary<_,_>()}
    let eventFatal (msg:string) = {Level = Microsoft.Extensions.Logging.LogLevel.Critical; Message = msg; Fields = new System.Collections.Generic.Dictionary<_,_>()}
    let setField (name:string) (value:obj) (msg:CustomMessage)=
        if not (msg.Fields.ContainsKey name) then
            msg.Fields.Add(name, value)
        msg
    let setFieldFromObject (name:string) (value:obj) (msg:CustomMessage)=
        if not (msg.Fields.ContainsKey name) then
            msg.Fields.Add(name, value)
        msg

module Logger = 
    let logSimple (l:Microsoft.Extensions.Logging.ILogger) (msg:CustomMessage) =
        l.Log(msg.Level, 0, msg, null, fun msg _ -> msg.ToString())
            
let writeLogSimple = Logger.logSimple
