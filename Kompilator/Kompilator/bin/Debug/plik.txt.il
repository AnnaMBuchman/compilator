.assembly extern mscorlib { }
.assembly Kompilator { }
.method static void main()
{
.entrypoint
.try
{

// prolog
.locals init ( float64 niewidzialnetemp )
.locals init ( int32 niewidzialnetempint )

.locals init ( int32 za )
.locals init ( float64 zb )
ldc.i4 24
ldc.i4 5
add
stloc za
ldc.i4 27
neg
ldc.i4 99
add
conv.r8
stloc zb
ldloc za
call       void [mscorlib]System.Console::Write(int32)
nop
ldstr "\n"
call void [mscorlib]System.Console::Write(string)
ldloc zb
stloc niewidzialnetemp
ldloca.s niewidzialnetemp
ldstr  "0.000000"
call      class [mscorlib]        System.Globalization.CultureInfo[mscorlib] System.Globalization.CultureInfo::get_InvariantCulture()
call      instance string[mscorlib] System.Double::ToString(string, class [mscorlib] System.IFormatProvider)
call void [mscorlib]System.Console::Write(string)
nop
leave EndMain
}
catch [mscorlib]System.Exception
{
callvirt instance string [mscorlib]System.Exception::get_Message()
call void [mscorlib]System.Console::WriteLine(string)
leave EndMain
}
EndMain: ret
}
