using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    internal class CodeGenFieldInitLists
    {
        public readonly List<(FieldBuilder field, object value, bool isReadOnly)> ValueInitFields = new();
        public readonly List<(FieldBuilder field, Action<ILGenerator> code, bool isReadOnly)> CodeInitFields = new();

        public void Emit(ILGenerator il)
        {
            for (var i = 0; i < ValueInitFields.Count; ++i)
            {
                var (field, _, _) = ValueInitFields[i];
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem, typeof(object));
                il.Emit(OpCodes.Castclass, field.FieldType);
                il.Emit(OpCodes.Stfld, field);
            }
            foreach (var (field, code, _) in CodeInitFields)
            {
                il.Emit(OpCodes.Ldarg_0);
                code(il);
                il.Emit(OpCodes.Stfld, field);
            }
        }

        public object[] GetData()
        {
            return ValueInitFields.Select(f => f.value).ToArray();
        }
    }

    public class ILGeneratorEnv
    {
        public TypeBuilder TypeBuilder { get; }
        private readonly CodeGenFieldInitLists _fields;
        public ILGenerator ILGenerator { get; }

        internal ILGeneratorEnv(TypeBuilder typeBuilder, ILGenerator il, CodeGenFieldInitLists fields)
        {
            TypeBuilder = typeBuilder;
            ILGenerator = il;
            _fields = fields;
        }

        public FieldBuilder AddReadOnlyField<T>(string name, T obj)
        {
            return AddField(name, typeof(T), obj, isReadOnly: true);
        }

        public FieldBuilder AddField<T>(string name, T initValue)
        {
            return AddField(name, typeof(T), initValue, isReadOnly: false);
        }

        public FieldBuilder AddField(string name, Type objType, object obj, bool isReadOnly)
        {
            if (objType.IsValueType)
            {
                //We use Opcodes.Castclass, so we need to ensure it's a class.
                throw new ProtocolBuilderException("AddField only supports reference types.");
            }
            var attr = isReadOnly ? FieldAttributes.Private | FieldAttributes.InitOnly : FieldAttributes.Private;
            var field = TypeBuilder.DefineField(name, objType, attr);
            _fields.ValueInitFields.Add((field, obj, isReadOnly));
            return field;
        }

        public FieldBuilder AddField(string name, Type objType, Action<ILGenerator> valueGenerator, bool isReadOnly)
        {
            var attr = isReadOnly ? FieldAttributes.Private | FieldAttributes.InitOnly : FieldAttributes.Private;
            var field = TypeBuilder.DefineField(name, objType, attr);
            _fields.CodeInitFields.Add((field, valueGenerator, isReadOnly));
            return field;
        }
    }
}
