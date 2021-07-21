using System;
using System.Text;
using Newtonsoft.Json.Serialization;
using Stl.Internal;
using Stl.Reflection;
using Stl.Serialization.Internal;
using Stl.Text;

namespace Stl.Serialization
{
    public class TypeWritingUtf16Serializer : Utf16SerializerBase
    {
        private readonly ISerializationBinder _serializationBinder;
        private readonly StringBuilder _stringBuilder;

        public IUtf16Serializer Serializer { get; }
        public Func<Type, bool> TypeFilter { get; }

        public TypeWritingUtf16Serializer(IUtf16Serializer serializer, Func<Type, bool> typeFilter)
        {
            Serializer = serializer;
            TypeFilter = typeFilter;
            var serializationBinder = (Serializer as NewtonsoftJsonSerializer)?.Settings?.SerializationBinder;
#if NET5_0
            serializationBinder ??= SerializationBinder.Instance;
#else
            serializationBinder ??= CrossPlatformSerializationBinder.Instance;
#endif
            _serializationBinder = serializationBinder;
            _stringBuilder = new StringBuilder(256);
        }

        public override object? Read(string data, Type type)
        {
            _stringBuilder.Clear();
            var p = ListFormat.Default.CreateParser(data, _stringBuilder);

            p.ParseNext();
            if (string.IsNullOrEmpty(p.Item))
                // Special case: null deserialization
                return null;

            TypeNameHelpers.SplitAssemblyQualifiedName(p.Item, out var assemblyName, out var typeName);
            var actualType = _serializationBinder.BindToType(assemblyName, typeName);
            if (!TypeFilter.Invoke(actualType))
                throw Errors.UnsupportedTypeForJsonSerialization(actualType);
            if (!type.IsAssignableFrom(actualType))
                throw Errors.UnsupportedTypeForJsonSerialization(actualType);

            p.ParseNext();
            return Serializer.Reader.Read(p.Item, actualType);
        }

        public override string Write(object? value, Type type)
        {
            _stringBuilder.Clear();
            var f = ListFormat.Default.CreateFormatter(_stringBuilder);
            if (value == null) {
                // Special case: null serialization
                f.Append("");
                f.AppendEnd();
            }
            else {
                var actualType = value.GetType();
                if (!TypeFilter.Invoke(actualType))
                    throw Errors.UnsupportedTypeForJsonSerialization(type);
                var aqn = actualType.GetAssemblyQualifiedName(false, _serializationBinder);
                var json = Serializer.Writer.Write(value, type);
                f.Append(aqn);
                f.Append(json);
                f.AppendEnd();
            }
            return _stringBuilder.ToString();
        }
    }
}