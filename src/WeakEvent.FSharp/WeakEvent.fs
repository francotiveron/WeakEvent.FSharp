namespace WeakEvent.FSharp

open System
open WeakEvent

type IDelegateWeakEvent<'Delegate when 'Delegate :> System.Delegate> =
    abstract AddHandler: handler: 'Delegate -> unit
    abstract AddHandler: lifetimeObj: obj * handler: 'Delegate -> unit
    abstract RemoveHandler: handler: 'Delegate -> unit

type IWeakEvent<'Delegate,'Args when 'Delegate: delegate<'Args,unit> and 'Delegate :> Delegate> =
    inherit IObservable<'Args>
    inherit IDelegateWeakEvent<'Delegate>

type IWeakEvent<'Args> =
    inherit IWeakEvent<EventHandler<'Args>, 'Args>

[<CompiledName("FSharpWeakEvent`1")>]
type WeakEvent<'T>() =
    let weakEventSource = WeakEventSource<'T>()
     
    member this.Trigger(arg: 'T) = weakEventSource.Raise(this, arg)
    member this.Trigger(arg: 'T, exceptionHandler) = weakEventSource.Raise(this, arg, exceptionHandler)

    member __.Publish =
        { new IWeakEvent<'T> with
              member __.AddHandler(d) = weakEventSource.Subscribe(d)
              member __.AddHandler(l, d) = weakEventSource.Subscribe(l, d)
              member __.RemoveHandler(d) = weakEventSource.Unsubscribe(d)

              member this.Subscribe(observer) =
                  let h = new EventHandler<_>(fun _ args -> observer.OnNext(args))
                  (this :?> IWeakEvent<_>).AddHandler(h)

                  { new IDisposable with
                      member x.Dispose() =
                          (this :?> IWeakEvent<_>).RemoveHandler(h) } }
