using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;
using static Robust.Shared.Serialization.Manager.ISerializationManager;

namespace Content.Server._DV.NPC;

/// <summary>
///     Serializer used automatically for <see cref="MemoryId{TMemory}"/> types.
/// </summary>
/// <typeparam name="TMemory">The type of the memory for which the id is stored.</typeparam>
[TypeSerializer]
public sealed class MemoryIdSerializer<TMemory> : ITypeSerializer<MemoryId<TMemory>, ValueDataNode>, ITypeCopyCreator<MemoryId<TMemory>>
{
    public ValidationNode Validate(ISerializationManager serialization, ValueDataNode node, IDependencyCollection dependencies, ISerializationContext? context = null)
    {
        return new ValidatedValueNode(node);
    }

    public MemoryId<TMemory> Read(ISerializationManager serialization, ValueDataNode node, IDependencyCollection dependencies, SerializationHookContext hookCtx, ISerializationContext? context = null, InstantiationDelegate<MemoryId<TMemory>>? instanceProvider = null)
    {
        return new MemoryId<TMemory>(node.Value);
    }

    public DataNode Write(ISerializationManager serialization, MemoryId<TMemory> value, IDependencyCollection dependencies, bool alwaysWrite = false, ISerializationContext? context = null)
    {
        return new ValueDataNode(value.Id);
    }

    public MemoryId<TMemory> CreateCopy(ISerializationManager serializationManager, MemoryId<TMemory> source, IDependencyCollection dependencies, SerializationHookContext hookCtx, ISerializationContext? context = null)
    {
        return source;
    }
}
