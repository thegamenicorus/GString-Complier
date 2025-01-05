using Collections = System.Collections.Generic;
using Reflect = System.Reflection;
using Emit = System.Reflection.Emit;
using IO = System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using GStringCompiler.Extension;

namespace GStringCompiler
{
    public sealed class CodeGen
    {
        Emit.ILGenerator il = null;
        Collections.Dictionary<string, Emit.LocalBuilder> symbolTable;
        private Reflect.AssemblyName Name;
        private Emit.AssemblyBuilder AsmB;
        private Emit.ModuleBuilder ModB;
        private Project CurrentProject;
        private Class CurrentGenClass;
        private Method CurrentGenMethod;

        public CodeGen() { }
        public CodeGen(Stmt stmt, string moduleName)
        {
            if (IO.Path.GetFileName(moduleName) != moduleName)
            {
                throw new System.Exception("can only output into current directory!");
            }

            Reflect.AssemblyName name = new Reflect.AssemblyName(IO.Path.GetFileNameWithoutExtension(moduleName));
            Emit.AssemblyBuilder asmb = System.AppDomain.CurrentDomain.DefineDynamicAssembly(name, Emit.AssemblyBuilderAccess.Save);
            Emit.ModuleBuilder modb = asmb.DefineDynamicModule(moduleName);
            Emit.TypeBuilder typeBuilder = modb.DefineType("TestNamespace." + IO.Path.GetFileNameWithoutExtension(moduleName));

            Emit.MethodBuilder methb = typeBuilder.DefineMethod("main", Reflect.MethodAttributes.Static, typeof(void), System.Type.EmptyTypes);
            // CodeGenerator.
            this.il = methb.GetILGenerator();
            this.symbolTable = new Collections.Dictionary<string, Emit.LocalBuilder>();

            // Go Compile!
            this.GenStmt(stmt);

            il.Emit(Emit.OpCodes.Ret);

            typeBuilder.CreateType();
            modb.CreateGlobalFunctions();
            asmb.SetEntryPoint(methb);
            asmb.Save(moduleName);
            this.symbolTable = null;
            this.il = null;
        }

        public CodeGen(Project proj, string moduleName)
        {

            this.CurrentProject = proj;
            if (IO.Path.GetFileName(moduleName) != moduleName)            
                throw new Error("can only output into current directory!");

            Name = new Reflect.AssemblyName(IO.Path.GetFileNameWithoutExtension(moduleName));
            AsmB = System.AppDomain.CurrentDomain.DefineDynamicAssembly(Name, Emit.AssemblyBuilderAccess.Save);
            ModB = AsmB.DefineDynamicModule(moduleName);

            // Class masker.
            Mask(proj);

            // Create types.
            foreach (var c in proj.Classes)
            { 
                string ns = "";
                if (!string.IsNullOrEmpty(c.Namespace))
                    ns = c.Namespace + "." + c.Name;
                else
                    ns = c.Name;

                Emit.TypeBuilder typeBuilder = c.ClsBuilder;
                CurrentGenClass = c;
                foreach (var meth in c.Methods)
                {
                    if (c.MethodBuilderKeeper.Count > 0)
                    {
                        var builder = c.MethodBuilderKeeper.Where(i => i.BuiltMethod.Name.Equals(meth.Name) && i.Parameters.SequenceEqual(meth.MethodParams.Select(met => met.ParameterType).ToArray()));                        
                        // Need to combine into class.
                    }
                    
                    CurrentGenMethod = meth;
                    TypeIdentifier.LoadMethod(meth);

                    var mBuilder = c.MethodBuilderKeeper.SingleOrDefault(m => m.BuiltMethod.Name.Equals(meth.Name) && m.Parameters.SequenceEqual(meth.MethodParams.Select(met => met.ParameterType).ToArray()));

                    if (mBuilder != null)
                    {
                        Emit.MethodBuilder methb = mBuilder.BuiltMethod;
                        this.il = methb.GetILGenerator();
                        this.symbolTable = new Collections.Dictionary<string, Emit.LocalBuilder>();
                        
                        // Go Compile!
                        this.GenStmt(meth.Body);
                        il.Emit(Emit.OpCodes.Ret);

                        if (meth.IsEntryMethod)
                            AsmB.SetEntryPoint(methb);
                        c.MethodBuilderKeeper.Add(new BuiltMethodAndParam() { BuiltMethod = methb, Parameters = meth.MethodParams.Select(met => met.ParameterType).ToArray() });
                        this.symbolTable = null;
                        this.il = null;
                    }
                    else
                    {
                        var param = string.Join(",", Enumerable.Range(0, meth.MethodParams.Count()).Select(i => "(" + meth.MethodParams[i].ParameterType.Name + ")" + meth.MethodParams[i].ParamName).ToArray());
                        throw new Error("Not found method: '" + meth.Name + "' ,parameters: '" + param + "' in class: '" + ns + "'");
                    }
                }
                typeBuilder.CreateType();
            }          

            
            ModB.CreateGlobalFunctions();
            AsmB.Save(moduleName);
        }

        public void Mask(Project proj)
        {
            foreach (var c in proj.Classes)
            {
                string ns = "";
                if (!string.IsNullOrEmpty(c.Namespace))
                    ns = c.Namespace + "." + c.Name;
                else
                    ns = c.Name;

                Emit.TypeBuilder typeBuilder = null;
                if (c.ClassAccessLevel != AccessLevel.Private)
                {
                    Reflect.TypeAttributes attr = Reflect.TypeAttributes.Public;
                    typeBuilder = ModB.DefineType(ns, attr);
                }
                else                
                    typeBuilder = ModB.DefineType(ns);                                   
                c.ClsBuilder = typeBuilder;
                foreach (var meth in c.Methods)
                {
                    if (c.MethodBuilderKeeper.Count > 0)
                    {
                        var cl = c.MethodBuilderKeeper.Where(i => i.BuiltMethod.Name.Equals(meth.Name) && i.Parameters.SequenceEqual(meth.MethodParams.Select(met => met.ParameterType).ToArray()));
                        // Need to combine into class.
                        if (cl.Count() > 0)
                        {
                            var param = string.Join(",", Enumerable.Range(0, meth.MethodParams.Count()).Select(i => "(" + meth.MethodParams[i].ParameterType.Name + ")" + meth.MethodParams[i].ParamName).ToArray());
                            throw new Error("Method: '" + meth.Name + "' ,parameters: '" + param + "' in class: '" + ns + "' is duplicated");
                        }
                    }
                    
                    Reflect.MethodAttributes metAttr = default(Reflect.MethodAttributes);

                    switch (meth.AccessLevel)
                    {
                        case AccessLevel.Public:
                            metAttr = Reflect.MethodAttributes.Public;
                            break;
                        case AccessLevel.Protected:
                            metAttr = Reflect.MethodAttributes.Family;
                            break;
                        case AccessLevel.Private:
                            metAttr = Reflect.MethodAttributes.Private;
                            break;
                    }

                    if (meth.IsStatic)
                        metAttr = metAttr | Reflect.MethodAttributes.Static;

                    Emit.MethodBuilder methb = typeBuilder.DefineMethod(meth.Name, metAttr, meth.ReturnType, meth.MethodParams.Select(met => met.ParameterType).ToArray());

                    for (int i = 0; i < meth.MethodParams.Count(); i++)
                        methb.DefineParameter(i+1,Reflect.ParameterAttributes.None,meth.MethodParams[i].ParamName); // need change for pass by ref

                    c.MethodBuilderKeeper.Add(new BuiltMethodAndParam() { BuiltMethod = methb,ReferenceGSMethodModel = meth, Parameters = meth.MethodParams.Select(met => met.ParameterType).ToArray() });
                }               
            }
        }

        private void GenStmt(Stmt stmt)
        {
            if (stmt is Sequence)
            {
                Sequence seq = (Sequence)stmt;
                this.GenStmt(seq.First);
                this.GenStmt(seq.Second);
            }
            else if (stmt == null) { }
            #region DeclareLocalVar
            else if (stmt is DeclareLocalVar)
            {
                // declare a local
                DeclareLocalVar declare = (DeclareLocalVar)stmt;
                if (declare.Expr != null)
                {
                    var type = this.TypeOfExpr(declare.Expr);

                    if (type != null && declare.FixedType != null && !type.IsArray)
                    {
                        var fixedType = declare.FixedType == typeof(System.Text.StringBuilder) ? typeof(string) : declare.FixedType;
                        if (type != fixedType)
                        {
                            throw new Error("Cannot implicitly convert type '" + type.Name + "' to '" + fixedType + "', on declare variable '" + declare.Ident + "'");
                        }
                    }


                    if (type == null)
                        type = declare.FixedType;
                    this.symbolTable[declare.Ident] = this.il.DeclareLocal(type);
                    Assign assign = new Assign();
                    assign.Ident = declare.Ident;
                    assign.Expr = declare.Expr;
                    this.GenStmt(assign);
                }
                else
                    if (declare.FixedType == typeof(System.Text.StringBuilder))
                    this.symbolTable[declare.Ident] = this.il.DeclareLocal(typeof(string));
                else
                    this.symbolTable[declare.Ident] = this.il.DeclareLocal(declare.FixedType);
                // set the initial value            
            }
            #endregion
            #region Assign
            else if (stmt is Assign)
            {
                Assign assign = (Assign)stmt;

                if (TypeIdentifier.IsArray(assign.Ident) && assign.Index != null)
                {
                    LoadArrayIndex(assign.Ident, assign.Index);
                    AssignCollection ac = new AssignCollection { Ident = assign.Ident, TypeOfExpr = TypeOfExpr(TypeIdentifier.GetArrayExpr(assign.Ident)) };
                    this.GenExpr(assign.Expr, this.TypeOfExpr(assign.Expr));
                    StoreLocalArray(ac);
                }
                else
                {
                    var type = this.TypeOfExpr(assign.Expr);

                    //if (type != null && assign.TypeOfReferencedVar != null)
                    //{
                    //    var fixedType = assign.TypeOfReferencedVar == typeof(System.Text.StringBuilder) ? typeof(string) : assign.TypeOfReferencedVar;
                    //    if (type != fixedType)
                    //    {
                    //        throw new Error("Cannot implicitly convert type '" + type.Name + "' to '" + fixedType + "', on assign variable '" + assign.Ident + "'");
                    //    }
                    //}

                    if (type == null)
                        type = assign.TypeOfReferencedVar;
                    if (type == null && assign.Expr is SequenceExpr)
                        type = //TypeIdentifier.
                            GetExprTypeFromTree((assign.Expr as SequenceExpr).Value);

                    var metParam = TypeIdentifier.GetDetailOfParam(assign.Ident);
                    if (metParam != null && metParam.IsPassByRef)
                        if (metParam.IsMethodStatic)
                            this.il.Emit(Emit.OpCodes.Ldarg, metParam.Order); //for static
                        else
                            this.il.Emit(Emit.OpCodes.Ldarg, metParam.Order + 1); // for normal

                    if (type == typeof(string) && assign.Expr is SequenceExpr)
                    {
                        //check if max concat is not more than 4
                        var concat_list = (assign.Expr as SequenceExpr).Value.ShuntingYardArtmValue;
                        if (concat_list.Count < 5)
                        {//normal operation
                            this.GenExpr(assign.Expr, type);
                            this.Store(assign.Ident, type);
                        }
                        else
                        {
                            this.symbolTable["Assign_Concat_String"] = this.il.DeclareLocal(typeof(object).MakeArrayType(1));
                            this.GenExtraStringConcat(concat_list.Cast<object>().ToList(), "Assign_Concat_String");
                            this.Store(assign.Ident, type);
                        }
                    }
                    else
                    {
                        this.GenExpr(assign.Expr, type);
                        this.Store(assign.Ident, type);
                    }

                }
                if (assign.Expr is GStringCollection && (assign.Expr as GStringCollection).Element != null)
                {
                    if ((assign.Expr as GStringCollection).Element != null)
                    {
                        AssignCollectionValue(assign.Expr as GStringCollection);

                        //just test
                        //var s = assign.Expr as LocalArray;
                        //this.il.Emit(Emit.OpCodes.Ldloc, symbolTable[s.Ident]);
                        //this.il.Emit(Emit.OpCodes.Ldc_I4_0);
                        //this.il.Emit(Emit.OpCodes.Ldc_I4_0);
                        //var call = TypeOfExpr(s).GetMethod("Get");
                        //var print = typeof(System.Console).GetMethod("Write", new System.Type[] { typeof(int) });
                        //this.il.Emit(Emit.OpCodes.Call, call);
                        //this.il.Emit(Emit.OpCodes.Call, print);
                    }
                }
            }
            #endregion
            #region AssignCollection
            else if (stmt is AssignCollection)
            {
                AssignCollection assign = (AssignCollection)stmt;
                if (assign.CollectionType == CollectionType.Array)
                {
                    var f = this.symbolTable[assign.Ident];
                    this.il.Emit(Emit.OpCodes.Ldloc, this.symbolTable[assign.Ident]);
                    foreach (var item in assign.Index)
                        //if (item is int)
                        this.il.Emit(Emit.OpCodes.Ldc_I4, item);
                    //else if (item is Variable)
                    //    this.GenExpr((Variable)item,typeof(int));

                    //if (assign.Value.GetType() == assign.TypeOfExpr.GetElementType())
                    //{
                    //    if (assign.Value is int)
                    //        this.il.Emit(Emit.OpCodes.Ldc_I4, (int)assign.Value);
                    //    else if (assign.Value is string)
                    //        this.il.Emit(Emit.OpCodes.Ldstr, (string)assign.Value);
                    //    else if (assign.Value is float)
                    //        this.il.Emit(Emit.OpCodes.Ldc_R4, (float)assign.Value);
                    //    else if (assign.Value is double)
                    //        this.il.Emit(Emit.OpCodes.Ldc_R8, (double)assign.Value);
                    //}
                    if (TypeOfExpr((Expr)assign.Value) == assign.TypeOfExpr.GetElementType())
                    {
                        GenExpr((Expr)assign.Value, TypeOfExpr((Expr)assign.Value));
                    }
                    //if (assign.Value is Parser.get)
                    //{
                    //    if (assign.Value is int)
                    //        this.il.Emit(Emit.OpCodes.Ldc_I4, (int)assign.Value);
                    //    else if (assign.Value is string)
                    //        this.il.Emit(Emit.OpCodes.Ldstr, (string)assign.Value);
                    //    else if (assign.Value is float)
                    //        this.il.Emit(Emit.OpCodes.Ldc_R4, (float)assign.Value);
                    //    else if (assign.Value is double)
                    //        this.il.Emit(Emit.OpCodes.Ldc_R8, (double)assign.Value);
                    //}
                    else if (assign.Value is Variable)
                        GenExpr(assign.Value as Variable, typeof(int));

                    StoreLocalArray(assign);
                }
                else if (assign.CollectionType == CollectionType.List)
                {
                }
            }
            #endregion
            #region Condition
            else if (stmt is ConditionHolder)
            {
                ConditionHolder conditionHolder = stmt as ConditionHolder;
                var condition = (conditionHolder.Condition as SequenceExpr);
                //if (condition.Value.JumpOut == default(Emit.Label))
                Emit.Label jumpWhile = il.DefineLabel();

                //Go back to this condition, for while
                if (conditionHolder.IsLoop)
                    this.il.MarkLabel(jumpWhile);

                condition.Value.JumpOut = this.il.DefineLabel();
                GenExprCondition(conditionHolder);

                if (conditionHolder.IsLoop)
                    this.il.Emit(Emit.OpCodes.Br, jumpWhile);

                this.il.MarkLabel(condition.Value.JumpOut);
            }
            #endregion
            #region Display
            else if (stmt is Display)
            {
                // the "print" statement is an alias for System.Console.WriteLine. 
                // it uses the string case
                Display display = stmt as Display;
                if (display.DisplayObjArr.Element == null)
                {
                    this.GenExpr(display.Expr, typeof(string));
                }
                else
                {
                    var display_name = display.DisplayObjArr.Ident + Guid.NewGuid();
                    this.symbolTable[display_name] = this.il.DeclareLocal(typeof(object).MakeArrayType(1));
                    this.GenExtraStringConcat(display.DisplayObjArr.Element, display_name);
                }

                Reflect.MethodInfo write;
                if (display.WithLine)
                    write = typeof(System.Console).GetMethod("WriteLine", new System.Type[] { typeof(string) });
                else
                    write = typeof(System.Console).GetMethod("Write", new System.Type[] { typeof(string) });
                this.il.Emit(Emit.OpCodes.Call, write);
            }
            #endregion
            #region ReadValue
            else if (stmt is ReadValue)
            {
                ReadValue reader = stmt as ReadValue;
                if (reader.Index != null)
                {
                    this.il.Emit(Emit.OpCodes.Ldloc, symbolTable[reader.Ident]);
                    foreach (object item in reader.Index)
                        if (item is int)
                            this.il.Emit(Emit.OpCodes.Ldc_I4, (int)item);
                        else //if (item is Variable)
                        {
                            GenExpr(item as Expr, typeof(int));
                        }
                    this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("ReadLine", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { }, null));
                    if (reader.ReadType == typeof(int))
                        this.il.Emit(Emit.OpCodes.Call, typeof(int).GetMethod("Parse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(string) }, null));

                    //Only array, other collection need to find method
                    //var g = reader.ReadType.MakeArrayType(reader.Index.Count);
                    var call = reader.ReadType.MakeArrayType(reader.Index.Count).GetMethod("Set");
                    this.il.Emit(Emit.OpCodes.Call, call);
                }
                else
                {
                    this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("ReadLine", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { }, null));
                    if (reader.ReadType == typeof(int))//need to check more type
                        this.il.Emit(Emit.OpCodes.Call, typeof(int).GetMethod("Parse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(string) }, null));
                    if (reader.Ident != null)
                        this.Store(reader.Ident, reader.ReadType);
                }
            }
            #endregion
            #region Repeat[Loop]
            #region Repeat since [For loop]
            else if (stmt is ForLoop)
            {
                ForLoop forLoop = (ForLoop)stmt;
                this.symbolTable[forLoop.Ident] = this.il.DeclareLocal(typeof(int));
                Assign assign = new Assign();
                assign.Ident = forLoop.Ident;
                assign.Expr = forLoop.From;
                this.GenStmt(assign);

                // jump to the check
                Emit.Label check = this.il.DefineLabel();
                this.il.Emit(Emit.OpCodes.Br, check);
                // statements in the body of the for loop                
                Emit.Label body = this.il.DefineLabel();
                this.il.MarkLabel(body);
                this.GenStmt(forLoop.Body);

                // to (increment the value of x)
                this.il.Emit(Emit.OpCodes.Ldloc, this.symbolTable[forLoop.Ident]);

                //this.il.Emit(Emit.OpCodes.Ldc_I4, 1);
                GenExpr(forLoop.runningValue, typeof(int));
                if (forLoop.runnerType == LoopRunningExpr.Increment)
                    this.il.Emit(Emit.OpCodes.Add);
                else
                    this.il.Emit(Emit.OpCodes.Sub);

                this.Store(forLoop.Ident, typeof(int));

                // **test** does x equal 100? (do the test)
                this.il.MarkLabel(check);
                this.il.Emit(Emit.OpCodes.Ldloc, this.symbolTable[forLoop.Ident]);
                this.GenExpr(forLoop.To, typeof(int));
                if (forLoop.runnerType == LoopRunningExpr.Increment)
                    if (forLoop.IsToBefore)
                        this.il.Emit(Emit.OpCodes.Blt, body);
                    else
                        this.il.Emit(Emit.OpCodes.Ble, body);
                else
                    if (forLoop.IsToBefore)
                    this.il.Emit(Emit.OpCodes.Bgt, body);
                else
                    this.il.Emit(Emit.OpCodes.Bge, body);
            }
            #endregion
            #region Repeat while
            else if (stmt is WhileLoop)
            {
                WhileLoop whileLoop = (WhileLoop)stmt;
                GenStmt(whileLoop.ConditionHolder);
            }
            #endregion
            #region Repeat do-while
            else if (stmt is DoLoop)
            {
                DoLoop doLoop = (DoLoop)stmt;
                ConditionHolder conditionHolder = new ConditionHolder();
                conditionHolder.IsLoop = true;
                conditionHolder.IsDoWhileStyle = true;
                conditionHolder.Condition = doLoop.Condition;
                conditionHolder.IfTrue = doLoop.Body;
                //GenExprCondition(conditionHolder);
                GenStmt(conditionHolder);
            }
            #endregion
            #endregion
            else if (stmt is CallMethod)
            {
                CallMethod callMethod = stmt as CallMethod;

                SetPromptCallMethod(callMethod);

                if (callMethod.PromptCallMethod != null)
                {
                    if (GSType.IsValueType(callMethod.RefType) && !callMethod.PromptCallMethod.IsStatic && callMethod.RefType != null)
                    {
                        symbolTable["cm_temp"] = this.il.DeclareLocal(callMethod.RefType);
                        this.il.Emit(Emit.OpCodes.Stloc, symbolTable["cm_temp"]);
                        this.il.Emit(Emit.OpCodes.Ldloca, symbolTable["cm_temp"]);
                    }

                    System.Reflection.ParameterInfo[] paramType = null;
                    try
                    {
                        paramType = callMethod.PromptCallMethod.GetParameters();
                    }
                    catch { }

                    for (int i = 0; i < callMethod.MethodArgs.Count(); i++)
                    {
                        var ma = callMethod.MethodArgs[i];
                        GenExpr(ma.ArgsExpr, ma.ArgsType, false, ma.IsPassByRef);
                        if (paramType != null && ma.ArgsType != typeof(object) && paramType[i].ParameterType == typeof(object))
                            this.il.Emit(Emit.OpCodes.Box, ma.ArgsType);
                    }

                    if (!callMethod.PromptCallMethod.IsStatic && (callMethod.PromptCallMethod.IsVirtual || callMethod.IsVirtCall))
                        this.il.Emit(Emit.OpCodes.Callvirt, callMethod.PromptCallMethod);
                    else
                        this.il.Emit(Emit.OpCodes.Call, callMethod.PromptCallMethod);

                }

                if (callMethod.NextCallMethod != null)
                {
                    GenStmt(callMethod.NextCallMethod);
                }
            }
            else if (stmt is Return)
            {
                Return ret = stmt as Return;
                this.GenExpr(ret.Expr, CurrentGenMethod.ReturnType);
            }
            else
            {
                throw new Error("don't know how to gen a " + stmt.GetType().Name);
            }
        }

        private Reflect.MethodInfo GetExpectMethod(CallMethod callMethod)
        {
            Type[] argsType = callMethod.MethodArgs.Select(s => s.ArgsType).ToArray();
            if (argsType.Where(c => c == null).Count() > 0)
            {
                argsType = new Type[callMethod.MethodArgs.Select(s => s.ArgsType).Count()];
                for (int i = 0; i < argsType.Length; i++)
                {
                    if (callMethod.MethodArgs[i].ArgsType != null)
                        argsType[i] = callMethod.MethodArgs[i].ArgsType;
                    else
                    {
                        argsType[i] = TypeOfExpr(callMethod.MethodArgs[i].ArgsExpr);
                        if (callMethod.MethodArgs[i].IsPassByRef)
                            argsType[i] = argsType[i].ToReferenceType();

                    }
                }
            }

            for (int i = 0; i < argsType.Length; i++)
            {
                if (callMethod.MethodArgs[i].IsPassByRef)
                    argsType[i] = argsType[i].ToReferenceType();
            }

            bool isStatic = CurrentGenMethod.IsStatic;
            var res = CurrentGenClass.Methods.Where(met => met.Name.Equals(callMethod.Ident) && met.IsStatic == isStatic
                       && met.MethodParams.Select(i => i.IsPassByRef).SequenceEqual(callMethod.MethodArgs.Select(s => s.IsPassByRef))
                       && met.MethodParams.Select(i => i.ParameterType).SequenceEqual(argsType)
                       );

            if (res.Count() == 1)
            {
                return CurrentGenClass.MethodBuilderKeeper.SingleOrDefault(m => m.ReferenceGSMethodModel == res.First()).BuiltMethod;                
            }
            else if (res.Count() > 1)
            {
                throw new Error("Method: '" + callMethod.Ident + "' in class: '" + CurrentGenClass.Name + "' is duplicated");
            }
            else
            {
                if (callMethod.RefType != null)
                {
                    return GSType.GetMethodFromRefType(callMethod.RefType, callMethod.Ident, argsType,CurrentGenMethod.IsStatic);
                }
            }

            return null;
        }

        private void GenExprCondition(ConditionHolder conditionHolder)
        {
            if (conditionHolder.Condition is SequenceExpr)
            {
                var condition = (conditionHolder.Condition as SequenceExpr);
                condition.Value.JumpToCode = this.il.DefineLabel();
                // Jump to else if/else.
                condition.Value.JumpNextMainCondition = this.il.DefineLabel();


                var andElseContainer = new Stack<ConditionSymbolStackItem>();
                var jumpLabel = new Stack<Emit.Label>();

                if (conditionHolder.IsDoWhileStyle)
                {
                    this.il.MarkLabel(condition.Value.JumpToCode);
                    GenStmt(conditionHolder.IfTrue);

                    if (!conditionHolder.IsLoop)
                        this.il.Emit(Emit.OpCodes.Br, condition.Value.JumpOut);

                    if (conditionHolder.IfFalse != null)
                    {
                        // Define next condition.
                        this.il.MarkLabel(condition.Value.JumpNextMainCondition);
                        if (conditionHolder.IfFalse is ConditionHolder)
                        {
                            var interestConn = ((SequenceExpr)((ConditionHolder)conditionHolder.IfFalse).Condition).Value;
                            interestConn.JumpOut = condition.Value.JumpOut;
                        }
                        // Maybe a new sequence or a new condition.
                        GenStmt(conditionHolder.IfFalse);
                    }
                    GenExprConditionSyn(condition.Value, ExprCondition.None, ExprCondition.None,
                       conditionHolder.IfFalse != null, andElseContainer, jumpLabel);
                }
                else
                {
                    GenExprConditionSyn(condition.Value, ExprCondition.None, ExprCondition.None,
                        conditionHolder.IfFalse != null, andElseContainer, jumpLabel);

                    this.il.MarkLabel(condition.Value.JumpToCode);
                    GenStmt(conditionHolder.IfTrue);

                    if (!conditionHolder.IsLoop)
                        this.il.Emit(Emit.OpCodes.Br, condition.Value.JumpOut);

                    if (conditionHolder.IfFalse != null)
                    {
                        // Define next condition.
                        this.il.MarkLabel(condition.Value.JumpNextMainCondition);
                        if (conditionHolder.IfFalse is ConditionHolder)
                        {
                            var interestConn = ((SequenceExpr)((ConditionHolder)conditionHolder.IfFalse).Condition).Value;
                            interestConn.JumpOut = condition.Value.JumpOut;
                        }
                        // Maybe a new sequence or a new condition.
                        GenStmt(conditionHolder.IfFalse);
                    }
                }
            }
        }

        // Next main condition = else if/ if.
        private enum ExprCondition { Left,None,Right}
        private enum ExprTypeCondition { AndAlso, OrElse, None }
        private void GenExprConditionSyn(
            TreeNode tree,
            ExprCondition mainStyle,
            ExprCondition subStyle,
            bool hasNextMainCondition,
            Stack<ConditionSymbolStackItem> andElseContainer,
            Stack<Emit.Label> jumpLabel)
        {
            if (tree.Op != null)
            {

                if (tree.Op is OrElseSymbol || tree.Op is AndAlsoSymbol)
                {
                    andElseContainer.Push(new ConditionSymbolStackItem { Symbol = tree.Op, IsWalked = false });


                    tree.Left.JumpOut = tree.JumpOut;
                    tree.Right.JumpOut = tree.JumpOut;
                    tree.Left.JumpToCode = tree.JumpToCode;
                    tree.Right.JumpToCode = tree.JumpToCode;


                    tree.JumpNextPairCondition = this.il.DefineLabel();
                    // Jump to next else if/else.
                    tree.Left.JumpNextMainCondition = tree.JumpNextMainCondition;

                    // Save path jump.
                    jumpLabel.Push(tree.JumpNextPairCondition);

                    GenExprConditionSyn(tree.Left, (mainStyle == ExprCondition.None ? ExprCondition.Left : mainStyle), ExprCondition.Left, hasNextMainCondition, andElseContainer, jumpLabel);
                    andElseContainer.ElementAt(0).IsWalked = true;


                    // For main ||, flow not sequence should jump to code.
                    if (mainStyle == ExprCondition.None && tree.Op is OrElseSymbol && (tree.Left.Op is AndAlsoSymbol || tree.Left.Op is OrElseSymbol))
                        this.il.Emit(Emit.OpCodes.Br, tree.JumpToCode);

                    // Jump to next else if/else.
                    tree.Right.JumpNextMainCondition = tree.JumpNextMainCondition;
                    this.il.MarkLabel(tree.JumpNextPairCondition);

                    GenExprConditionSyn(tree.Right, (mainStyle == ExprCondition.None ? ExprCondition.Right : mainStyle), ExprCondition.Right, hasNextMainCondition, andElseContainer, jumpLabel);
                    andElseContainer.Pop();
                    jumpLabel.Pop();
                }

                // Check for +,-,*,/,%.
                else if (!(tree.Op is LogicalConditionSymbolExpr) && tree.Op is LogicalSymbolExpr)
                {
                    if (tree.Left.Value == null)
                        GenExprConditionSyn(tree.Left, mainStyle, subStyle, true, andElseContainer, jumpLabel);
                    else
                        GenExpr(tree.Left.Value, TypeOfExpr(tree.Left.Value));

                    if (tree.Right.Value == null)
                        GenExprConditionSyn(tree.Right, mainStyle, subStyle, true, andElseContainer, jumpLabel);
                    else
                        GenExpr(tree.Right.Value, TypeOfExpr(tree.Right.Value));
                    EmitLogical(tree.Op);
                }
                // Shoud be >,<,>=,<=,==,!==.
                else
                {
                    if (tree.Left.Value == null)
                        GenExprConditionSyn(tree.Left, mainStyle, subStyle, true, andElseContainer, jumpLabel);
                    else
                        GenExpr(tree.Left.Value, TypeOfExpr(tree.Left.Value));

                    if (tree.Right.Value == null)
                        GenExprConditionSyn(tree.Right, mainStyle, subStyle, true, andElseContainer, jumpLabel);
                    else
                        GenExpr(tree.Right.Value, TypeOfExpr(tree.Right.Value));

                    Emit.OpCode opcodeJump = default(Emit.OpCode);


                    // Define jump.
                    Emit.Label labelJump = default(Emit.Label);

                    if (andElseContainer.Count == 0)
                    {
                        opcodeJump = GetJumpLogicalOpposite(tree.Op);
                        labelJump = hasNextMainCondition ? tree.JumpNextMainCondition : tree.JumpOut;
                    }
                    else if (andElseContainer.ElementAt(0).Symbol is OrElseSymbol)
                    {
                        if (subStyle == ExprCondition.Left)
                        {
                            opcodeJump = GetJumpLogical(tree.Op);
                            if (andElseContainer.Count == 1)
                                labelJump = tree.JumpToCode;
                            else
                                labelJump = FindNextJump(true, andElseContainer, jumpLabel, 1, tree.JumpToCode, tree.JumpNextMainCondition, tree.JumpOut, hasNextMainCondition);
                        }
                        else if (subStyle == ExprCondition.Right)
                        {
                            opcodeJump = GetJumpLogicalOpposite(tree.Op);
                            if (andElseContainer.Count == 1)
                                if (hasNextMainCondition)
                                    labelJump = tree.JumpNextMainCondition;
                                else
                                    labelJump = tree.JumpOut;
                            else
                                labelJump = FindNextJump(false, andElseContainer, jumpLabel, 1, tree.JumpToCode, tree.JumpNextMainCondition, tree.JumpOut, hasNextMainCondition);
                        }
                    }
                    else if (andElseContainer.ElementAt(0).Symbol is AndAlsoSymbol)
                    {
                        if (subStyle == ExprCondition.Left)
                        {
                            opcodeJump = GetJumpLogicalOpposite(tree.Op);
                            if (andElseContainer.Count == 1)
                                if (hasNextMainCondition)
                                    labelJump = tree.JumpNextMainCondition;
                                else
                                    labelJump = tree.JumpOut;
                            else
                                labelJump = FindNextJump(false, andElseContainer, jumpLabel, 1, tree.JumpToCode, tree.JumpNextMainCondition, tree.JumpOut, hasNextMainCondition);
                        }
                        else if (subStyle == ExprCondition.Right)
                        {
                            opcodeJump = GetJumpLogicalOpposite(tree.Op);
                            if (andElseContainer.Count == 1)
                                if (hasNextMainCondition)
                                    labelJump = tree.JumpNextMainCondition;
                                else
                                    labelJump = tree.JumpOut;
                            else
                                labelJump = FindNextJump(false, andElseContainer, jumpLabel, 1, tree.JumpToCode, tree.JumpNextMainCondition, tree.JumpOut, hasNextMainCondition);
                        }
                    }
                    this.il.Emit(opcodeJump, labelJump);
                }
            }
            else
            {
                GenExpr(tree.Value, TypeOfExpr(tree.Value));
                GenExpr(new BooleanLiteral { Value = true }, typeof(bool));
                Emit.OpCode opcodeJump = default(Emit.OpCode);
                Emit.Label labelJump = default(Emit.Label);
                opcodeJump = GetJumpLogicalOpposite(new LogicalEqualSymbol());
                labelJump = hasNextMainCondition ? tree.JumpNextMainCondition : tree.JumpOut;
                this.il.Emit(opcodeJump, labelJump);
            }
        }

        private Emit.Label FindNextJump(bool state,
            Stack<ConditionSymbolStackItem> andElseContainer,
            Stack<Emit.Label> jumpLabel,
            int index,
            Emit.Label jumpToCode,
            Emit.Label jumpNextMainCondition,
            Emit.Label jumpOut,
            bool hasNextMainCondition
            )
        {
            Emit.Label labelJump = default(Emit.Label);
            if (!state && andElseContainer.ElementAt(index).Symbol is AndAlsoSymbol)
            {
                if (index == andElseContainer.Count - 1)
                    if (hasNextMainCondition)
                        labelJump = jumpNextMainCondition;
                    else
                        labelJump = jumpOut;
                else
                {
                    for (int i = 1; i < andElseContainer.Count; i++)
                    {
                        if (index + i < andElseContainer.Count)
                        {
                            if (!andElseContainer.ElementAt(index + i).IsWalked)
                            {
                                labelJump = FindNextJump(state, andElseContainer, jumpLabel, index + i, jumpToCode, jumpNextMainCondition, jumpOut, hasNextMainCondition);
                                break;
                            }
                        }
                        else if (hasNextMainCondition)
                        {
                            labelJump = jumpNextMainCondition;
                            break;
                        }
                        else
                        {
                            labelJump = jumpOut;
                            break;
                        }
                    }
                }
            }
            else if (!state && andElseContainer.ElementAt(index).Symbol is OrElseSymbol)
            {
                if (!andElseContainer.ElementAt(index).IsWalked)
                    labelJump = jumpLabel.ElementAt(index);
                else
                {
                    for (int i = 1; i < andElseContainer.Count; i++)
                    {
                        if (index + i < andElseContainer.Count)
                        {
                            if (!andElseContainer.ElementAt(index + i).IsWalked)
                            {
                                labelJump = FindNextJump(state, andElseContainer, jumpLabel, index + i, jumpToCode, jumpNextMainCondition, jumpOut, hasNextMainCondition);
                                break;
                            }
                        }
                        else if (hasNextMainCondition)
                        {
                            labelJump = jumpNextMainCondition;
                            break;
                        }
                        else
                        {
                            labelJump = jumpOut;
                            break;
                        }
                    }
                }
            }
            else if (state && andElseContainer.ElementAt(index).Symbol is OrElseSymbol)
            {
                if (index == andElseContainer.Count - 1)
                    if (hasNextMainCondition)
                        labelJump = jumpToCode;
                    else
                    {
                        for (int i = 1; i < andElseContainer.Count; i++)
                        {
                            if (index + i < andElseContainer.Count)
                            {
                                if (!andElseContainer.ElementAt(index + i).IsWalked)
                                {
                                    labelJump = FindNextJump(state, andElseContainer, jumpLabel, index + i, jumpToCode, jumpNextMainCondition, jumpOut, hasNextMainCondition);
                                    break;
                                }
                            }
                            else
                            {
                                labelJump = jumpToCode;
                                break;
                            }
                        }
                    }
            }
            else if (state && andElseContainer.ElementAt(index).Symbol is AndAlsoSymbol)
            {
                if (!andElseContainer.ElementAt(index).IsWalked)
                    labelJump = jumpLabel.ElementAt(index);
                else
                {
                    for (int i = 1; i < andElseContainer.Count; i++)
                    {
                        if (index + i < andElseContainer.Count)
                        {
                            if (!andElseContainer.ElementAt(index + i).IsWalked)
                            {
                                labelJump = FindNextJump(state, andElseContainer, jumpLabel, index + i, jumpToCode, jumpNextMainCondition, jumpOut, hasNextMainCondition);
                                break;
                            }
                        }
                        else
                        {
                            labelJump = jumpToCode;
                            break;
                        }
                    }
                }
            }
            return labelJump;
        }

        private Emit.OpCode GetJumpLogical(LogicalSymbolExpr symbol)
        {
            switch (symbol.GetType().Name)
            {
                case "MoreThanSymbol":
                    return Emit.OpCodes.Bgt;
                case "MoreThanAndEqualSymbol":
                    return Emit.OpCodes.Bge;
                case "LessThanSymbol":
                    return Emit.OpCodes.Blt;
                case "LessThanAndEqualSymbol":
                    return Emit.OpCodes.Ble;
                case "LogicalEqualSymbol":
                    return Emit.OpCodes.Beq;
                case "LogicalNotEqualSymbol":
                    return Emit.OpCodes.Bne_Un;
                default:
                    return Emit.OpCodes.Br;
            }
        }

        private Emit.OpCode GetJumpLogicalOpposite(LogicalSymbolExpr symbol)
        {
            switch (symbol.GetType().Name)
            {
                case "MoreThanSymbol":
                    return Emit.OpCodes.Ble;
                case "MoreThanAndEqualSymbol":
                    return Emit.OpCodes.Blt;
                case "LessThanSymbol":
                    return Emit.OpCodes.Bge;
                case "LessThanAndEqualSymbol":
                    return Emit.OpCodes.Bgt;
                case "LogicalEqualSymbol":
                    return Emit.OpCodes.Bne_Un;
                case "LogicalNotEqualSymbol":
                    return Emit.OpCodes.Beq;
                default:
                    return Emit.OpCodes.Br;
            }
        }

        private void LoadArrayIndex(string ident, System.Collections.Generic.List<object> position)
        {
            this.il.Emit(Emit.OpCodes.Ldloc, symbolTable[ident]);
            foreach (object item in position)
                if (item.GetType() == typeof(int))
                    this.il.Emit(Emit.OpCodes.Ldc_I4, (int)item);
                else
                    GenExpr(item as Expr, typeof(int));
        }

        private void AssignCollectionValue(GStringCollection expr)
        {
            AssignCollection assignCollection = new AssignCollection();
            if (expr is LocalArray)
            {
                LocalArray array = expr as LocalArray;
                assignCollection.Ident = array.Ident;
                assignCollection.TypeOfExpr = TypeOfExpr(array);
                assignCollection.CollectionType = CollectionType.Array;
                assignCollection.Index = new int[array.Size.Count];

                AssignCollectionValue(ref assignCollection, (array.Element.Count == 1 ? ((System.Collections.Generic.List<object>)array.Element[0]) : array.Element), 0);
            }
        }

        private void AssignCollectionValue(ref AssignCollection assignCollection, System.Collections.Generic.List<object> Element, int deep)
        {
            int locDeep = deep;
            for (int i = 0; i < Element.Count; i++)
            {
                assignCollection.Index[locDeep] = i;
                if (Element[i].GetType() == typeof(System.Collections.Generic.List<object>))
                {
                    deep++;
                    AssignCollectionValue(ref assignCollection, (System.Collections.Generic.List<object>)Element[i], deep);
                    deep = locDeep;
                }
                else
                {
                    assignCollection.Value = Element[i];
                    GenStmt(assignCollection);
                }
            }
        }

        private void Store(string name, System.Type type)
        {
            var metParam = TypeIdentifier.GetDetailOfParam(name);
            if (metParam != null)
            {
                var refType = (metParam.ParameterType == typeof(System.Text.StringBuilder) ? typeof(string) : type);
                if (refType == type)
                {
                    switch (metParam.IsPassByRef)
                    {
                        case true:
                            GenStoreParamIndex(metParam.ParameterType);
                            break;
                        case false:
                            if (metParam.IsMethodStatic)
                                this.il.Emit(Emit.OpCodes.Starg, metParam.Order);
                            else
                                this.il.Emit(Emit.OpCodes.Starg, metParam.Order + 1);
                            break;
                    }


                }
                else
                {
                    throw new Error("'" + name + "' is of type " + metParam.ParameterType.Name + " but attempted to store value of type " + type.Name);
                }
            }
            else if (this.symbolTable.ContainsKey(name))
            {
                Emit.LocalBuilder locb = this.symbolTable[name];

                if (locb.LocalType == type || type == null)
                {
                    this.il.Emit(Emit.OpCodes.Stloc, this.symbolTable[name]);
                }
                else
                {
                    throw new Error("'" + name + "' is of type " + locb.LocalType.Name + " but attempted to store value of type " + type.Name);
                }
            }
            else
            {
                throw new Error("Undeclared variable '" + name + "'");
            }
        }

        private void StoreLocalArray(AssignCollection localArray)
        {
            System.Type storeType = localArray.TypeOfExpr.GetElementType();
            if (this.symbolTable.ContainsKey(localArray.Ident))
            {
                System.Reflection.MethodInfo method = localArray.TypeOfExpr.GetMethod("Set");
                this.il.Emit(Emit.OpCodes.Call, method);
            }
            else
            {
                throw new Error("Undeclared variable '" + localArray.Ident + "'");
            }
        }

        private void GenExpr(Expr expr, System.Type expectedType, bool isNeedStaticConvert = true, bool loadByAddress = false)
        {
            System.Type deliveredType = null;

            if (expr is StringLiteral)
            {
                deliveredType = typeof(string);
                this.il.Emit(Emit.OpCodes.Ldstr, ((StringLiteral)expr).Value);
            }
            else if (expr is IntLiteral)
            {
                deliveredType = typeof(int);
                this.il.Emit(Emit.OpCodes.Ldc_I4, ((IntLiteral)expr).Value);
            }
            else if (expr is FloatLiteral)
            {
                deliveredType = typeof(float);
                this.il.Emit(Emit.OpCodes.Ldc_R4, ((FloatLiteral)expr).Value);
            }
            else if (expr is DoubleLiteral)
            {
                deliveredType = typeof(double);
                this.il.Emit(Emit.OpCodes.Ldc_R8, ((DoubleLiteral)expr).Value);
            }
            else if (expr is BooleanLiteral)
            {
                BooleanLiteral boolean = expr as BooleanLiteral;
                deliveredType = typeof(bool);
                switch (boolean.Value)
                {
                    case false:
                        this.il.Emit(Emit.OpCodes.Ldc_I4_0);
                        break;
                    case true:
                        this.il.Emit(Emit.OpCodes.Ldc_I4_1);
                        break;
                }

            }
            else if (expr is Variable)
            {
                Variable variable = expr as Variable;
                string ident = variable.Ident;
                var metParam = TypeIdentifier.GetDetailOfParam(variable.Ident);

                if (!this.symbolTable.ContainsKey(ident) && TypeIdentifier.GetDetailOfParam(variable.Ident) == null)
                {
                    throw new Error("Undeclared variable '" + ident + "'");
                }

                deliveredType = this.TypeOfExpr(expr);
                if (deliveredType.IsArray)
                    deliveredType = deliveredType.GetElementType();
                if (variable.Ornaments != null && variable.Ornaments.Count != 0)
                    foreach (Ornament ornament in variable.Ornaments)
                    {
                        if (ornament is VariableIndex)
                        {
                            LoadArrayIndex(variable.Ident, (ornament as VariableIndex).Index);
                            this.il.Emit(Emit.OpCodes.Call, TypeOfExpr(TypeIdentifier.GetArrayExpr(variable.Ident)).GetMethod("Get"));
                        }
                    }
                else if (metParam != null)
                {
                    // Individual from Assign.
                    if (metParam.IsMethodStatic)
                        // For static.
                        this.il.Emit(Emit.OpCodes.Ldarg, metParam.Order); 
                    else
                        // For normal.
                        this.il.Emit(Emit.OpCodes.Ldarg, metParam.Order + 1); 
                    // Assign not require to reference index.
                    if (metParam.IsPassByRef)
                        GenLoadParamIndex(metParam.ParameterType);
                }
                else if (loadByAddress)
                    this.il.Emit(Emit.OpCodes.Ldloca, this.symbolTable[ident]);
                else
                    this.il.Emit(Emit.OpCodes.Ldloc, this.symbolTable[ident]);
            }
            else if (expr is GStringCollection)
            {
                GStringCollection loc = expr as GStringCollection;
                deliveredType = loc.Type.MakeArrayType(loc.Size.Count);
                if (expr is LocalArray)
                {
                    GenExprArray(expr as LocalArray);
                }
            }
            else if (expr is SequenceExpr)
            {
                SequenceExpr sExpr = expr as SequenceExpr;
                if (sExpr.Value.ShuntingYardArtmValue.Where(i => i is LogicalConditionSymbolExpr).Count() == 0)
                {
                    GenExprTreeNodeNormalLogical(sExpr.Value, expectedType, isNeedStaticConvert, loadByAddress);
                }
                else
                {
                    if (expectedType == typeof(bool))
                    {
                        GenExprTreeNodeNormalLogical(sExpr.Value, expectedType, isNeedStaticConvert, loadByAddress);
                    }
                }

                if (expectedType == typeof(string) && sExpr.Value.ShuntingYardArtmValue.Count > 1 && sExpr.Value.ShuntingYardArtmValue.Where(x => x is EmptySymbol).Count() == 0)
                {
                    System.Type[] type = new System.Type[sExpr.Value.ShuntingYardArtmValue.Count];

                    var concatMethod = typeof(string).GetMethod("Concat", sExpr.Value.ShuntingYardArtmValue.Select(s => typeof(object)).ToArray());
                    this.il.Emit(Emit.OpCodes.Call, concatMethod);
                }

                if (expr is NotSymbolSequenceExpr)
                {
                    EmitLogical(new LogicalNotSymbol());
                }
                else if (expr is NegSymbolSequenceExpr)
                {
                    EmitLogical(new SubNegSymbol());
                }
            }
            else if (expr is OptStrSequenceExpr)
            {
                OptStrSequenceExpr sExpr = expr as OptStrSequenceExpr;
                if (sExpr.Value.ShuntingYardArtmValue.Where(i => i is LogicalConditionSymbolExpr).Count() == 0)
                {
                    GenExprTreeNodeNormalLogical(sExpr.Value, expectedType, false);
                    // TypeIdentifier.
                    deliveredType =
                        GetExprTypeFromTree(sExpr.Value);
                }
                else
                {
                    // TODO: Implement this.
                }
            }
            else if (expr is CallMethod)
            {
                CallMethod callmethod = expr as CallMethod;
                GenStmt(callmethod);
                deliveredType = TypeOfExpr(callmethod);
            }
            else if (expr is ReadValue)
            {
                ReadValue readVal = expr as ReadValue;
                GenStmt(readVal);
                deliveredType = TypeOfExpr(readVal);
            }
            else if (expr is Display)
            {
                Display display = expr as Display;
                GenStmt(display);
                deliveredType = TypeOfExpr(display);
            }
            else
            {
                throw new Error("GString compiler don't know how to generate " + expr.GetType().Name);
            }

            if (deliveredType != expectedType && isNeedStaticConvert)
            {
                ConvertStaticType(deliveredType, expectedType);
            }

        }

        private void GenLoadParamIndex(System.Type type)
        {
            if (type == Type.GetType("System.Boolean&"))
                this.il.Emit(Emit.OpCodes.Ldind_I1);
            else if (type == Type.GetType("System.Int16&"))
                this.il.Emit(Emit.OpCodes.Ldind_I2);
            else if (type == Type.GetType("System.Int32&"))
                this.il.Emit(Emit.OpCodes.Ldind_I4);
            else if (type == Type.GetType("System.Int64&"))
                this.il.Emit(Emit.OpCodes.Ldind_I8);
            else if (type == Type.GetType("System.Single&"))
                this.il.Emit(Emit.OpCodes.Ldind_R4);
            else if (type == Type.GetType("System.Double&"))
                this.il.Emit(Emit.OpCodes.Ldind_R8);
            else if (type == Type.GetType("System.String&"))
                this.il.Emit(Emit.OpCodes.Ldind_Ref);
        }

        private void GenStoreParamIndex(System.Type type)
        {
            if (type == Type.GetType("System.Boolean&"))
                this.il.Emit(Emit.OpCodes.Stind_I1);
            else if (type == Type.GetType("System.Int16&"))
                this.il.Emit(Emit.OpCodes.Stind_I2);
            else if (type == Type.GetType("System.Int32&"))
                this.il.Emit(Emit.OpCodes.Stind_I4);
            else if (type == Type.GetType("System.Int64&"))
                this.il.Emit(Emit.OpCodes.Stind_I8);
            else if (type == Type.GetType("System.Single&"))
                this.il.Emit(Emit.OpCodes.Stind_R4);
            else if (type == Type.GetType("System.Double&"))
                this.il.Emit(Emit.OpCodes.Stind_R8);
            else if (type == Type.GetType("System.String&"))
                this.il.Emit(Emit.OpCodes.Stind_Ref);
        }

        private void GenExprTreeNodeNormalLogical(TreeNode tree, System.Type expectedType, bool isNeedStaticConvert = true, bool isLoadByAddress = false)
        {
            if (tree.Op != null)
            {
                GenExprTreeNodeNormalLogical(tree.Left, expectedType, isNeedStaticConvert, isLoadByAddress);
                GenExprTreeNodeNormalLogical(tree.Right, expectedType, isNeedStaticConvert, isLoadByAddress);
                EmitLogical(tree.Op);
            }
            // For cut string sequence.
            else if (tree.Left != null && tree.Right != null)
            {
                GenExprTreeNodeNormalLogical(tree.Left, expectedType, isNeedStaticConvert, isLoadByAddress);
                GenExprTreeNodeNormalLogical(tree.Right, expectedType, isNeedStaticConvert, isLoadByAddress);
            }
            else
                GenExpr(tree.Value, expectedType, isNeedStaticConvert, isLoadByAddress);
        }

        private void EmitLogical(LogicalSymbolExpr symbol)
        {
            if (symbol is AddSymbol)
                this.il.Emit(Emit.OpCodes.Add);
            else if (symbol is SubSymbol)
                this.il.Emit(Emit.OpCodes.Sub);
            else if (symbol is MulSymbol)
                this.il.Emit(Emit.OpCodes.Mul);
            else if (symbol is DivSymbol)
                this.il.Emit(Emit.OpCodes.Div);
            else if (symbol is ModSymbol)
                this.il.Emit(Emit.OpCodes.Rem);
            else if (symbol is LogicalEqualSymbol)
                this.il.Emit(Emit.OpCodes.Ceq);
            else if (symbol is SubNegSymbol)
                this.il.Emit(Emit.OpCodes.Neg);
            else if (symbol is LogicalNotSymbol)
            {
                this.il.Emit(Emit.OpCodes.Ldc_I4_0);
                this.il.Emit(Emit.OpCodes.Ceq);
            }
            else if (symbol is LogicalNotEqualSymbol)
            {
                this.il.Emit(Emit.OpCodes.Ceq);
                this.il.Emit(Emit.OpCodes.Ldc_I4_0);
                this.il.Emit(Emit.OpCodes.Ceq);
            }
        }
        
        private void ConvertStaticType(System.Type deliveredType, System.Type expectedType)
        {
            if (deliveredType != null && deliveredType.FullName.Last() == '&')
                deliveredType = System.Type.GetType(deliveredType.FullName.Remove(deliveredType.FullName.Length - 1));
            if (expectedType == typeof(string) && GSType.ConvertStaticTypeChecker(deliveredType))
            {
                this.il.Emit(Emit.OpCodes.Box, deliveredType);
                this.il.Emit(Emit.OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
            }
            else
            {
                // TODO: Implement this.
            }
        }

        private void GenExtraStringConcat(System.Collections.Generic.List<object> displayElement, string ident)
        {

            this.il.Emit(Emit.OpCodes.Ldc_I4, displayElement.Count);
            this.il.Emit(Emit.OpCodes.Newarr, typeof(System.Object));
            this.il.Emit(Emit.OpCodes.Stloc, this.symbolTable[ident]);
            for (int i = 0; i < displayElement.Count; i++)
            {
                this.il.Emit(Emit.OpCodes.Ldloc, this.symbolTable[ident]);
                this.il.Emit(Emit.OpCodes.Ldc_I4, i);

                if (displayElement[i] is SequenceExpr)
                {
                    System.Type expectType;
                    var seq = displayElement[i] as SequenceExpr;

                    if ((displayElement[i] as SequenceExpr).Value.ShuntingYardArtmValue.Where(val => val is LogicalSymbolExpr).Count() == 0)
                        expectType = typeof(string);
                    else
                        expectType = TypeOfExpr(seq);

                    if (seq.Value.ShuntingYardArtmValue.Count < 5 || expectType != typeof(string))
                        this.GenExpr((Expr)displayElement[i], expectType);
                    else
                    {
                        var arrobj_gen_name = "Assign_Concat_String_" + Guid.NewGuid();
                        this.symbolTable[arrobj_gen_name] = this.il.DeclareLocal(typeof(object).MakeArrayType(1));
                        this.GenExtraStringConcat(seq.Value.ShuntingYardArtmValue.Cast<object>().ToList(), arrobj_gen_name);
                    }

                    // Do operation then convert value to string, because normal are (val1 to string) <operate> (val2 to string) = wrong result.
                    if (expectType != typeof(string))
                        ConvertStaticType(expectType, typeof(string));
                }
                else
                    this.GenExpr((Expr)displayElement[i], typeof(string));
                this.il.Emit(Emit.OpCodes.Stelem_Ref);
            }
            this.il.Emit(Emit.OpCodes.Ldloc, this.symbolTable[ident]);
            var concatMethod = typeof(string).GetMethod("Concat", new System.Type[] { typeof(object[]) });
            this.il.Emit(Emit.OpCodes.Call, concatMethod);
        }

        // Support int,string,var.
        private void GenExprArray(LocalArray expr)
        {
            int index = 0;
            System.Type[] paramType = new System.Type[expr.Size.Count];
            foreach (var item in expr.Size)
            {
                paramType[index] = typeof(int);
                if (item is IntLiteral || item is Variable || item is SequenceExpr)
                    GenExpr(item as Expr, typeof(int));
                else
                    throw new Error("Not support type: " + item.GetType());
                index++;
            }

            System.Reflection.ConstructorInfo opcodeArrayIns = expr.Type.MakeArrayType(expr.Size.Count).GetConstructor(paramType);
            this.il.Emit(Emit.OpCodes.Newobj, opcodeArrayIns);
        }

        private void SetPromptCallMethod(CallMethod callMethod)
        {
            if (callMethod.PromptCallMethod == null)
            {
                var expected_method = GetExpectMethod(callMethod);
                if (expected_method != null)
                {
                    callMethod.PromptCallMethod = expected_method;
                    callMethod.ReturnType = expected_method.ReturnType;
                    if (callMethod.NextCallMethod != null)
                        callMethod.NextCallMethod.RefType = callMethod.ReturnType;
                }
                else
                {
                    var param = string.Join(",", Enumerable.Range(0, callMethod.MethodArgs.Count()).Select(i => "(" + callMethod.MethodArgs[i].ArgsType.Name + ")").ToArray());
                    throw new Error("Method: '" + callMethod.Ident + "' with types: '" + param + "' in class: '" + CurrentGenClass.Name + "' is undefined");
                }
            }
        }

        public System.Type GetExprTypeFromTree(TreeNode tree)
        {
            System.Type result = null;
            if (tree.ShuntingYardArtmValue != null && tree.ShuntingYardArtmValue.Where(c => c is LogicalEqualSymbol || c is LogicalNotEqualSymbol
                || c is LogicalNotSymbol || c is MoreThanAndEqualSymbol || c is MoreThanSymbol || c is LessThanAndEqualSymbol || c is LessThanSymbol)
                .Count() > 0)
            {
                result = typeof(bool);
            }
            else if (tree.Op != null)
            {
                result = GetExprTypeFromTree(tree.Left);
                if (result == null)
                    result = GetExprTypeFromTree(tree.Right);
            }
            // For cut string sequence.
            else if (tree.Left != null && tree.Right != null)
            {
                if (result == null)
                    result = GetExprTypeFromTree(tree.Right);
            }
            else
                result = TypeOfExpr(tree.Value);

            return result;
        }

        // Change in this don't forgot to change in TypeIdentifier.
        public System.Type TypeOfExpr(Expr expr)
        {
            if (expr is StringLiteral)
            {
                return typeof(string);
            }
            else if (expr is IntLiteral)
            {
                return typeof(int);
            }
            else if (expr is FloatLiteral)
            {
                return typeof(float);
            }
            else if (expr is DoubleLiteral)
            {
                return typeof(double);
            }
            else if (expr is BooleanLiteral)
            {
                return typeof(bool);
            }
            else if (expr is Variable)
            {
                Variable var = (Variable)expr;

                var metParam = TypeIdentifier.GetDetailOfParam(var.Ident);
                if (metParam != null)
                {
                    if (var.Ornaments != null && var.Ornaments.Count > 0)
                        return metParam.ParameterType.GetElementType();
                    return metParam.ParameterType;
                }
                else if (this.symbolTable.ContainsKey(var.Ident))
                {
                    Emit.LocalBuilder locb = this.symbolTable[var.Ident];
                    // If assign array in integer var, their will have ornament.
                    if (var.Ornaments != null && var.Ornaments.Count > 0)
                        return locb.LocalType.GetElementType();
                    return locb.LocalType;
                }
                else
                {
                    throw new Error("variable: " + var.Ident + " is not declared");
                }
            }
            else if (expr is LocalArray)
            {
                LocalArray loc = expr as LocalArray;
                return loc.Type.MakeArrayType(loc.Size.Count);
            }
            else if (expr is SequenceExpr)
            {
                var sexpr = expr as SequenceExpr;
                bool isContainString = sexpr.Value.ShuntingYardArtmValue.Where(i => i is StringLiteral).Count() > 0;
                if (isContainString)
                {
                    return typeof(string);
                }
                else
                    // TypeIdentifier.
                    return GetExprTypeFromTree(sexpr.Value);
            }
            else if (expr is OptStrSequenceExpr)
            {
                OptStrSequenceExpr sexpr = expr as OptStrSequenceExpr;
                bool isContainString = sexpr.Value.ShuntingYardArtmValue.Where(i => i is StringLiteral).Count() > 0;
                if (isContainString)
                {
                    return typeof(string);
                }
                else
                    //TypeIdentifier.
                    return GetExprTypeFromTree(sexpr.Value);
            }
            else if (expr is CallMethod)
            {
                CallMethod cm = expr as CallMethod;

                CallMethod dev_cm = cm;
                SetPromptCallMethod(dev_cm);
                while (dev_cm.NextCallMethod != null)
                {
                    dev_cm = dev_cm.NextCallMethod;
                    SetPromptCallMethod(dev_cm);
                }


                if (dev_cm.PromptCallMethod != null)
                    return dev_cm.PromptCallMethod.ReturnType;
                else
                    // Method created internal.
                    return GetExpectMethod(dev_cm).ReturnType;
            }
            else if (expr is ReadValue || expr is Display)
                return typeof(string);
            else
            {
                // Throw unsupported type.
                throw new Error("GString compiler don't know how to generate " + expr.GetType().Name);
            }
        }
    }
}