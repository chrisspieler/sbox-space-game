using Sandbox;
using System;

[AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
[CodeGenerator( CodeGeneratorFlags.WrapMethod | CodeGeneratorFlags.Static | CodeGeneratorFlags.WrapPropertySet, "CheatManager.ReportCheat" )]
public class CheatAttribute : Attribute { }
