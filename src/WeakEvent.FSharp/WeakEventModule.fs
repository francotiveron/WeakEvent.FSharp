namespace WeakEvent.FSharp

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module WeakEvent =
    [<CompiledName("Create")>]
    let create<'T> () =
        let ev = new WeakEvent<'T>()
        ev.Trigger, ev.Publish

    [<CompiledName("Map")>]
    let map mapping (sourceEvent: IWeakEvent<'Delegate, 'T>) =
        let ev = new WeakEvent<_>()
        sourceEvent.Add(fun x -> ev.Trigger(mapping x))
        ev.Publish

    [<CompiledName("Filter")>]
    let filter predicate (sourceEvent: IWeakEvent<'Delegate, 'T>) =
        let ev = new WeakEvent<_>()

        sourceEvent.Add(fun x ->
            if predicate x then
                ev.Trigger x)

        ev.Publish

    [<CompiledName("Partition")>]
    let partition predicate (sourceEvent: IWeakEvent<'Delegate, 'T>) =
        let ev1 = new WeakEvent<_>()
        let ev2 = new WeakEvent<_>()

        sourceEvent.Add(fun x ->
            if predicate x then
                ev1.Trigger x
            else
                ev2.Trigger x)

        ev1.Publish, ev2.Publish

    [<CompiledName("Choose")>]
    let choose chooser (sourceEvent: IWeakEvent<'Delegate, 'T>) =
        let ev = new WeakEvent<_>()

        sourceEvent.Add(fun x ->
            match chooser x with
            | None -> ()
            | Some r -> ev.Trigger r)

        ev.Publish

    [<CompiledName("Scan")>]
    let scan collector state (sourceEvent: IWeakEvent<'Delegate, 'T>) =
        let mutable state = state
        let ev = new WeakEvent<_>()

        sourceEvent.Add(fun msg ->
            let z = state
            let z = collector z msg
            state <- z
            ev.Trigger(z))

        ev.Publish

    [<CompiledName("Add")>]
    let add callback (sourceEvent: IWeakEvent<'Delegate, 'T>) =
        sourceEvent.Add(callback)

    [<CompiledName("Pairwise")>]
    let pairwise (sourceEvent: IWeakEvent<'Delegate, 'T>) : IWeakEvent<'T * 'T> =
        let ev = new WeakEvent<'T * 'T>()
        let mutable lastArgs = None

        sourceEvent.Add(fun args2 ->
            (match lastArgs with
             | None -> ()
             | Some args1 -> ev.Trigger((args1, args2)))

            lastArgs <- Some args2)

        ev.Publish

    [<CompiledName("Merge")>]
    let merge (event1: IWeakEvent<'Del1, 'T>) (event2: IWeakEvent<'Del2, 'T>) =
        let ev = new WeakEvent<_>()
        event1.Add(fun x -> ev.Trigger(x))
        event2.Add(fun x -> ev.Trigger(x))
        ev.Publish

    [<CompiledName("Split")>]
    let split (splitter: 'T -> Choice<'U1, 'U2>) (sourceEvent: IWeakEvent<'Delegate, 'T>) =
        let ev1 = new WeakEvent<_>()
        let ev2 = new WeakEvent<_>()

        sourceEvent.Add(fun x ->
            match splitter x with
            | Choice1Of2 y -> ev1.Trigger(y)
            | Choice2Of2 z -> ev2.Trigger(z))

        ev1.Publish, ev2.Publish
