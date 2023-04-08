namespace Microsoft.FSharp.Control

open System
open WeakEvent

type IWeakEvent<'Args> =
    inherit IEvent<EventHandler<'Args>, 'Args>
    abstract AddHandler: lifetimeObj: obj * handler: EventHandler<'Args> -> unit

[<CompiledName("FSharpWeekEvent`1")>]
type WeakEvent<'T>() =
    let weakEventSource = WeakEventSource<'T>()

    member this.Trigger(arg: 'T) = weakEventSource.Raise(this, arg)
    member this.Raise = this.Trigger

    member __.Publish = {
        new IWeakEvent<'T> with
            member __.AddHandler(d) = weakEventSource.Subscribe(d)
            member __.AddHandler(l, d) = weakEventSource.Subscribe(l, d)
            member __.RemoveHandler(d) = weakEventSource.Unsubscribe(d)

            member this.Subscribe(observer) =
                let h = EventHandler<_>(fun _ args -> observer.OnNext(args))
                (this :?> IEvent<_, _>).AddHandler(h)
                { new System.IDisposable with member __.Dispose() = (this :?> IEvent<_, _>).RemoveHandler(h) } }

module Event = 
    let addWeak callback lifetimeObj (sourceEvent: IWeakEvent<'Args>) =
        sourceEvent.AddHandler(lifetimeObj, EventHandler<_>(callback))