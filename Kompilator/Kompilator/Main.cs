
using System;
using System.IO;
using System.Collections.Generic;


namespace Kompilator
{

    public class Identyficator
    {
        public Typ typ;
        public bool is_value = false;
        public object value;
        
        public Identyficator(Typ t)
        {
            typ = t;
        }

    }
    class Kompilator
    {
        public static bool is_ret = false;
        public static int nr_etykiety = 0;
        
        public static Dictionary<string,Identyficator> indetyfikatory = new Dictionary<string, Identyficator>();

        private static List<string> source;
        public static int errors = 0;

        static int Main(string[] args)
        {
            string file="plik.txt";

            FileStream source;

            if (args.Length >= 1)
                file = args[0];
            
            try
            {
                var sr = new StreamReader(file);
                string str = sr.ReadToEnd();
                sr.Close();
                Kompilator.source = new System.Collections.Generic.List<string>(str.Split(new string[] { "\r\n" }, System.StringSplitOptions.None));
                source = new FileStream(file, FileMode.Open);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                return 1;
            }
            Scanner scanner = new Scanner(source);
            Parser parser = new Parser(scanner);



            sw = new StreamWriter(file + ".il");
            parser.Parse();
            if (errors == 0)
            {
                GenProlog();
                if (parser.root != null)
                {
                    parser.root.CheckType();
                    if (errors == 0)
                    {
                        
                        parser.root.GenCode();
                        Console.WriteLine("  compilation successful\n");
                       
                        
                    }
                    
                }
                else Console.WriteLine("  compilation successful\n");
                GenEpilog();
                sw.Close();
            }
            else
            {
                sw.Close();
                Console.WriteLine($"\n  {errors} errors detected\n");
                File.Delete(file + ".il");
            }
           
            
            source.Close();
            return errors == 0 ? 0 : 2;
        }
        public static void EmitCode(string instr = null)
        {
            sw.WriteLine(instr);
        }

        public static void EmitCode(string instr, params object[] args)
        {
            sw.WriteLine(instr, args);
        }

        private static StreamWriter sw;

        private static void GenProlog()
        {
            EmitCode(".assembly extern mscorlib { }");
            EmitCode(".assembly Kompilator { }");
            EmitCode(".method static void main()");
            EmitCode("{");
            EmitCode(".entrypoint");
            EmitCode(".try");
            EmitCode("{");
            EmitCode();

            EmitCode("// prolog");
            EmitCode(".locals init ( float64 niewidzialnetemp )");
            EmitCode(".locals init ( int32 niewidzialnetempint )");


            EmitCode();
        }

        private static void GenEpilog()
        {
            if(is_ret)
            {
                EmitCode("ereturn: nop");
            }
            EmitCode("leave EndMain");
            EmitCode("}");
            EmitCode("catch [mscorlib]System.Exception");
            EmitCode("{");
            EmitCode("callvirt instance string [mscorlib]System.Exception::get_Message()");
            EmitCode("call void [mscorlib]System.Console::WriteLine(string)");
            EmitCode("leave EndMain");
            EmitCode("}");
            EmitCode("EndMain: ret");
            EmitCode("}");
        }



    }
    public enum Typ { Int, Double, Bool, Iden }

    public abstract class SyntaxTree
    {

        public Typ type;
        public int line = -1;
        public string val;
        public bool is_asi = false;
        public abstract void CheckType();
        public abstract void GenCode();
    }

    public class Program : SyntaxTree
    {
        public SyntaxTree block;
        public Program(SyntaxTree blo, int l)
        {
            block = blo;
            line = l;
        }

        public override void CheckType()
        {
            type = block.type;
            block.CheckType();
        }

        public override void GenCode()
        {
            // Kompilator.EmitCode();
            block.GenCode();
        }
    }
    public class Block : SyntaxTree
    {
        public SyntaxTree declars;
        public SyntaxTree instrs;
        public Block(SyntaxTree dec, SyntaxTree ins, int l)
        {
            declars = dec;
            instrs = ins;
            line = l;
        }
        public Block(SyntaxTree dec, int l)
        {
            declars = dec;
            //  instrs = ins;
            line = l;
        }
        public Block(int l, SyntaxTree ins)
        {
            //  declars = dec;
            instrs = ins;
            line = l;
        }

        public override void CheckType()
        {
            if (declars != null)
                declars.CheckType();
            if (instrs != null)
                instrs.CheckType();
        }

        public override void GenCode()
        {
            if (declars != null)
                declars.GenCode();
            if (instrs != null)
                instrs.GenCode();
        }
    }
    public class Declars : SyntaxTree
    {
        public SyntaxTree declars;
        public SyntaxTree declar;
        public Declars(SyntaxTree decs, SyntaxTree dec, int l)
        {
            declars = decs;
            declar = dec;
            line = l;
        }

        public override void CheckType()
        {
            if (declars != null)
                declars.CheckType();
            declar.CheckType();
        }

        public override void GenCode()
        {
            if (declars != null)
                declars.GenCode();
            declar.GenCode();
        }
    }
    public class Instrs : SyntaxTree
    {
        public SyntaxTree instrs;
        public SyntaxTree instr;
        public Instrs(SyntaxTree inss, SyntaxTree ins, int l)
        {
            instrs = inss;
            instr = ins;
            line = l;
        }
        public Instrs( SyntaxTree ins, int l)
        {
            
            instr = ins;
            line = l;
        }
        public Instrs(int l,SyntaxTree inss)
        {

            instrs = inss;
            line = l;
        }

        public override void CheckType()
        {
            if (instrs != null)
                instrs.CheckType();
            if(instr!=null)
                instr.CheckType();
        }

        public override void GenCode()
        {
            if (instrs != null)
                instrs.GenCode();
            if(instr!=null)
                instr.GenCode();
        }
    }
    public class Declar : SyntaxTree
    {
        public Typ typ;
        public string ident;

        public Declar(Typ t, string id, int l)
        {
            typ = t;
            ident = "z"+id;
            line = l;
        }

        public override void CheckType()
        {
            if (Kompilator.indetyfikatory.ContainsKey(ident))
            {
                Console.WriteLine("variable already declared duplicate in line {0}", line);
                Kompilator.errors++;

            }
            else
                Kompilator.indetyfikatory.Add(ident, new Identyficator(typ));
        }

        public override void GenCode()
        {
            string t;
            if (typ == Typ.Int)
            {
                t = "int32";
            }
            else if (typ == Typ.Double)
            {
                t = "float64";
            }
            else
                t = "bool";

            Kompilator.EmitCode(".locals init ( " + t + " " + ident + " )");
        }
    }

    class IntNumber : SyntaxTree
    {



        public IntNumber(string v, int l) { val = v; line = l; }

        public override void CheckType()
        {
            type = Typ.Int;
        }

        public override void GenCode()
        {
            Kompilator.EmitCode("ldc.i4 {0}", val);
        }

    }

    class RealNumber : SyntaxTree
    {


        public RealNumber(string v, int l) { val = v; line = l; }

        public override void CheckType()
        {
            type = Typ.Double;
        }

        public override void GenCode()
        {
            Kompilator.EmitCode("ldc.r8 {0}", val);
        }

    }

    class Ident : SyntaxTree
    {



        public Ident(string s, int l) { val = "z"+ s; line = l; }

        public override void CheckType()
        {
            if (!Kompilator.indetyfikatory.ContainsKey(val))
            {
                Console.WriteLine("[IDENT] undeclared variable line {0}", line);
                Kompilator.errors++;
            }
            type = Typ.Iden;
        }

        public override void GenCode()
        {
            Kompilator.EmitCode("ldloc " + val);
        }

    }
    class Bool : SyntaxTree
    {

        private bool bo;

        public Bool(bool b, int l)
        {
            bo = b;
            if (bo)
                val = "1";
            else
                val = "0";
            line = l;
        }

        public override void CheckType()
        {
            type = Typ.Bool;
        }

        public override void GenCode()
        {
            if (bo)
                Kompilator.EmitCode("ldc.i4.1");
            else
                Kompilator.EmitCode("ldc.i4.0");
        }

    }

    public class CondIf : SyntaxTree
    {
        public SyntaxTree logic;
        public SyntaxTree instr;
        public CondIf(SyntaxTree l, SyntaxTree ins, int lo)
        {
            logic = l;
            instr = ins;
            line = lo;
        }

        public override void CheckType()
        {
            logic.CheckType();
            if (instr != null)
            {
                instr.CheckType();
               
            }
                if (logic.type == Typ.Iden)
                {
                    if (!Kompilator.indetyfikatory.ContainsKey(logic.val))
                    {
                        // Console.WriteLine("undeclared variable line {0}", line);
                        // Kompilator.errors++;
                    }
                    else if (Kompilator.indetyfikatory[logic.val].typ != Typ.Bool)
                    {
                        Console.Write("[IF] wrong type in line {0}", line);
                        Kompilator.errors++;
                    }
                }
                else if (logic.type != Typ.Bool)
                {
                    Console.WriteLine("[IF]  wrong type of condition in line {0}", line);
                    Kompilator.errors++;

                }
            }
        

        public override void GenCode()
        {
            logic.GenCode();
            if(logic.is_asi)
            {
                Kompilator.EmitCode("ldloc " + logic.val);
            }
            //Kompilator.EmitCode("stloc niewidzialnetemp");
            //Kompilator.EmitCode("ldloc.s niewidzialnetemp");
            int nr = Kompilator.nr_etykiety;
            Kompilator.nr_etykiety++;
            Kompilator.EmitCode("brfalse e{0}", nr);
            if (instr != null)
            {
                instr.GenCode();
                if (instr.is_asi)
                {
                    Kompilator.EmitCode("ldloc " + instr.val);
                }
            }
            Kompilator.EmitCode("e{0}: nop", nr);

        }
    }
    public class CondIfElse : SyntaxTree
    {
        public SyntaxTree logic;
        public SyntaxTree instr;
        public SyntaxTree instr2;
        public CondIfElse(SyntaxTree l, SyntaxTree ins, SyntaxTree ins2, int li)
        {
            logic = l;
            instr = ins;
            instr2 = ins2;
            line = li;
        }

        public override void CheckType()
        {
            logic.CheckType();
            if(instr!=null)
              instr.CheckType();
            if(instr2!=null)
              instr2.CheckType();
            if (logic.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(logic.val))
                {
                    // Console.WriteLine("undeclared variable line {0}", line);
                    // Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[logic.val].typ != Typ.Bool)
                {
                    Console.Write("[IF ELSE] wrong type of condition in line {0}", line);
                    Kompilator.errors++;
                }
            }
            else if (logic.type != Typ.Bool)
            {
                Console.WriteLine("[IF ELSE]wrong type in condition in line {0}", line);
                Kompilator.errors++;
            }
        }

        public override void GenCode()
        {
            logic.GenCode();
            if (logic.is_asi)
            {
                Kompilator.EmitCode("ldloc " + logic.val);
            }
            //Kompilator.EmitCode("stloc niewidzialnetemp");
            //Kompilator.EmitCode("ldloc.s niewidzialnetemp");
            int nr = Kompilator.nr_etykiety;
            Kompilator.EmitCode("brfalse e{0}", nr);
            if (instr != null)
            {
                instr.GenCode();
                if (instr.is_asi)
                {
                    Kompilator.EmitCode("ldloc " + instr.val);
                }
            }
            Kompilator.EmitCode("br e{0}", nr + 1);
            Kompilator.EmitCode("e{0}: nop", nr);
            Kompilator.nr_etykiety++;
            if (instr2 != null)
            {
                instr2.GenCode();
                if (instr2.is_asi)
                {
                    Kompilator.EmitCode("ldloc " + instr2.val);
                }
            }


            Kompilator.EmitCode("e{0}: nop", nr + 1);
            Kompilator.nr_etykiety++;
        }
    }
    public class While : SyntaxTree
    {
        public SyntaxTree logic;
        public SyntaxTree instr;
        public While(SyntaxTree l, SyntaxTree ins, int li)
        {
            logic = l;
            instr = ins;
            line = li;
        }
        public While(SyntaxTree l,int li)
        {
            logic = l;
            line = li;
        }

        public override void CheckType()
        {
            logic.CheckType();
            if(instr!=null)
            instr.CheckType();
            if (logic.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(logic.val))
                {
                    // Console.WriteLine("undeclared variable line {0}", line);
                    // Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[logic.val].typ != Typ.Bool)
                {
                    Console.Write("[WHILE] wrong type in line {0}", line);
                    Kompilator.errors++;
                }
            }
            else if (logic.type != Typ.Bool)
            {
                Console.WriteLine("[WHILE] wrong type of condition in line {0}", line);
                Kompilator.errors++;

            }
        }

        public override void GenCode()
        {
            int nre = Kompilator.nr_etykiety;
            Kompilator.nr_etykiety++;
            Kompilator.nr_etykiety++;
            Kompilator.EmitCode("br e{0}", nre);

            Kompilator.EmitCode("e{0}: nop", nre + 1);
            if (instr != null)
            {
                instr.GenCode();
                if (instr.is_asi)
                {
                    Kompilator.EmitCode("ldloc " + instr.val);
                }
            }


            Kompilator.EmitCode("e{0}: nop", nre);
            logic.GenCode();
            if (logic.is_asi)
            {
                Kompilator.EmitCode("ldloc " + logic.val);
            }

            Kompilator.EmitCode("brtrue e{0}", nre + 1);
            //    instr.GenCode();



        }
    }

    public class Bit : SyntaxTree
    {
        public SyntaxTree bitexp;
        public SyntaxTree term;
        public string sig;
        public Bit(SyntaxTree b, string s, SyntaxTree t, int l)
        {
            bitexp = b;
            term = t;
            line = l;
            sig = s;
        }

        public override void CheckType()
        {
            bitexp.CheckType();
            term.CheckType();
            if (bitexp.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(bitexp.val))
                {
                    Console.WriteLine("undeclared variable line {0}", line);
                    Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[bitexp.val].typ != Typ.Int)
                {
                    Console.WriteLine("wrong type in line {0}", line);
                    Kompilator.errors++;
                }
            }
            else if (bitexp.type != Typ.Int)
            {
                Console.WriteLine("wrong type in line {0}", line);
                Kompilator.errors++;

            }
            if (term.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(term.val))
                {
                    Console.WriteLine("undeclared variable line {0}", line);
                    Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[term.val].typ != Typ.Int)
                {
                    Console.WriteLine("wrong type in line {0}", line);
                    Kompilator.errors++;
                }
            }
            else if (term.type != Typ.Int)
            {
                Console.WriteLine("wrong type in line {0}", line);
                Kompilator.errors++;

            }

            type = Typ.Int;
        }

        public override void GenCode()
        {
            bitexp.GenCode();
            if (bitexp.is_asi)
            {
                Kompilator.EmitCode("ldloc " + bitexp.val);
            }

            term.GenCode();
            if (term.is_asi)
            {
                Kompilator.EmitCode("ldloc " + term.val);
            }
            if (sig == "|")
            {
                Kompilator.EmitCode("or");
            }
            else
                Kompilator.EmitCode("and");

        }
    }
    public class ToDouble : SyntaxTree
    {
        public SyntaxTree bitexp;
        public ToDouble(SyntaxTree b, int l)
        {
            bitexp = b;
            line = l;
        }

        public override void CheckType()
        {
            bitexp.CheckType();
            if (bitexp.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(bitexp.val))
                {
                    Console.WriteLine("undeclared variable line {0}", line);
                    Kompilator.errors++;
                }
            }
            type = Typ.Double;
        }

        public override void GenCode()
        {
            bitexp.GenCode();
            if (bitexp.is_asi)
            {
                Kompilator.EmitCode("ldloc " + bitexp.val);
            }
            Kompilator.EmitCode("conv.r8");
        }
    }
    public class ToInt : SyntaxTree
    {
        public SyntaxTree bitexp;
        public ToInt(SyntaxTree b, int l)
        {
            bitexp = b;
            line = l;
        }

        public override void CheckType()
        {
            if (bitexp.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(bitexp.val))
                {
                    Console.WriteLine("undeclared variable line {0}", line);
                    Kompilator.errors++;
                }
            }
            type = Typ.Int;
        }

        public override void GenCode()
        {
            bitexp.GenCode();
            if (bitexp.is_asi)
            {
                Kompilator.EmitCode("ldloc " + bitexp.val);
            }
            Kompilator.EmitCode("conv.i4");
        }
    }
    public class UnarTilda : SyntaxTree
    {
        public SyntaxTree bitexp;
        public UnarTilda(SyntaxTree b, int l)
        {
            bitexp = b;
            line = l;
        }

        public override void CheckType()
        {
            bitexp.CheckType();
            if (bitexp.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(bitexp.val))
                {
                    Console.WriteLine("undeclared variable line {0}", line);
                    Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[bitexp.val].typ != Typ.Int)
                {
                    Console.WriteLine("wrong type in line {0}", line);
                    Kompilator.errors++;
                }
            }
            else if (bitexp.type != Typ.Int)
            {
                Console.WriteLine("wrong type in line {0}", line);
                Kompilator.errors++;

            }
            type = Typ.Int;
        }

        public override void GenCode()
        {
            bitexp.GenCode();
            if (bitexp.is_asi)
            {
                Kompilator.EmitCode("ldloc " + bitexp.val);
            }
            Kompilator.EmitCode("not");
        }
    }
    public class UnarNot : SyntaxTree
    {
        public SyntaxTree bitexp;
        public UnarNot(SyntaxTree b, int l)
        {
            bitexp = b;
            line = l;
        }

        public override void CheckType()
        {
            bitexp.CheckType();
            if (bitexp.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(bitexp.val))
                {
                    Console.WriteLine("undeclared variable line {0}", line);
                    Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[bitexp.val].typ != Typ.Bool)
                {
                    Console.WriteLine("wrong type in line {0}", line);
                    Kompilator.errors++;
                }
            }
            else if (bitexp.type != Typ.Bool)
            {
                Console.WriteLine("wrong type in line {0}", line);
                Kompilator.errors++;

            }
            type = Typ.Bool;
        }

        public override void GenCode()
        {
            bitexp.GenCode();
            if (bitexp.is_asi)
            {
                Kompilator.EmitCode("ldloc " + bitexp.val);
            }
            Kompilator.EmitCode("ldc.i4.0");
            Kompilator.EmitCode("ceq");
        }
    }
    public class UnarMinus : SyntaxTree
    {
        public SyntaxTree factor;
        public UnarMinus(SyntaxTree b, int l)
        {
            factor = b;
            line = l;
        }

        public override void CheckType()
        {
            factor.CheckType();
            line = factor.line;
            if (factor.type == Typ.Bool)
            {
                Console.Write("wrong type bool inr line {0}", line);
                Kompilator.errors++;
            }
            else if (factor.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(factor.val))
                {
                    Console.WriteLine("undeclared variable line {0}", line);
                    Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[factor.val].typ == Typ.Bool)
                {
                    Console.Write("wrong type bool inr line {0}", line);
                    Kompilator.errors++;
                }
                else
                {
                    type = Kompilator.indetyfikatory[factor.val].typ;
                }
            }
            else
            {
                type = factor.type;
                val = "-" + factor.val;
            }

        }


        public override void GenCode()
        {
            factor.GenCode();
            if (factor.is_asi)
            {
                Kompilator.EmitCode("ldloc " + factor.val);
            }
            Kompilator.EmitCode("neg");
        }
    }
    public class Assign : SyntaxTree
    {


        public string Ident;
        public SyntaxTree factor;
        public Assign(string id, SyntaxTree f, int l)
        {
            line = l;
            val=Ident = "z"+ id;
            factor = f;
            is_asi = true;
        }

        public override void CheckType()
        {
            factor.CheckType();
            if (!Kompilator.indetyfikatory.ContainsKey(Ident))
            {
                Console.WriteLine("[ASSIGNE] undeclared variable in line {0}", line);
                Kompilator.errors++;
            }
            else
            {
                Identyficator id = Kompilator.indetyfikatory[Ident];

                if (id.typ != factor.type)
                {
                    if (id.typ == Typ.Double && factor.type == Typ.Int)
                    {

                    }
                    else
                    {
                        Console.WriteLine("[ASSIGNE] wrong type in line {0}", line);
                        Kompilator.errors++;
                    }
                }
            }

            type = Typ.Iden;
        }

        public override void GenCode()
        {
            factor.GenCode();
            if (factor.is_asi)
            {
                Kompilator.EmitCode("ldloc " + factor.val);
            }
            Identyficator id = Kompilator.indetyfikatory[Ident];
            if (id.typ == Typ.Double && factor.type == Typ.Int)
            {
                Kompilator.EmitCode("conv.r8");
            }


            Kompilator.EmitCode("stloc " + Ident);
            val = Ident;
            //Kompilator.EmitCode("ldloc " + Ident);
        }
    }
    public class Relation : SyntaxTree
    {


        public SyntaxTree adding;
        public SyntaxTree rel;
        public string sig;
        Typ tadd;
        Typ trel;
        public Relation(SyntaxTree r, string s, SyntaxTree l, int li)
        {
            adding = l;
            sig = s;
            rel = r;
            line = li;
        }

        public override void CheckType()
        {
            adding.CheckType();
            rel.CheckType();
            if (sig != "==" && sig != "!=")
            {
                if (adding.type == Typ.Bool || rel.type == Typ.Bool)
                {
                    Console.WriteLine("[RELATION] Wrong type in line {0}", line);
                    Kompilator.errors++;
                }
                else if (adding.type == Typ.Iden)
                {
                    if (!Kompilator.indetyfikatory.ContainsKey(adding.val))
                    {
                        Console.WriteLine("[RELATION] undeclared variable line {0}", line);
                        Kompilator.errors++;
                    }
                    else if (Kompilator.indetyfikatory[adding.val].typ == Typ.Bool)
                    {
                        Console.Write("[RELATION] wrong type in line {0}", line);
                        Kompilator.errors++;
                    }
                    else
                    {
                        tadd = Kompilator.indetyfikatory[adding.val].typ;
                    }
                }
                else
                {
                    tadd = adding.type;
                }
                 if (rel.type == Typ.Iden)
                {
                    if (!Kompilator.indetyfikatory.ContainsKey(rel.val))
                    {
                        Console.WriteLine("[RELATION] undeclared variable line {0}", line);
                        Kompilator.errors++;
                    }
                    else if (Kompilator.indetyfikatory[rel.val].typ == Typ.Bool)
                    {
                        Console.Write("[RELATION] wrong type in line {0}", line);
                        Kompilator.errors++;
                    }
                    else
                    {
                        trel=Kompilator.indetyfikatory[rel.val].typ;
                    }
                }
                else
                {
                    trel = rel.type;
                }
                
            }
            else
            {
                
                if(adding.type==Typ.Iden)
                {
                    if (!Kompilator.indetyfikatory.ContainsKey(adding.val))
                    {
                        Console.WriteLine("[RELATION] undeclared variable line {0}", line);
                        Kompilator.errors++;
                    }
                    else 
                    {
                        tadd = Kompilator.indetyfikatory[adding.val].typ;
                    }
                }
                else
                {
                    tadd = adding.type;
                }

                if (rel.type == Typ.Iden)
                {
                    if (!Kompilator.indetyfikatory.ContainsKey(rel.val))
                    {
                        Console.WriteLine("[RELATION] undeclared variable line {0}", line);
                        Kompilator.errors++;
                    }
                    else
                    {
                        trel = Kompilator.indetyfikatory[rel.val].typ;
                    }
                }
                else
                {
                    trel = rel.type;
                }
                if((tadd == Typ.Bool && trel != Typ.Bool) || (tadd != Typ.Bool && trel == Typ.Bool))                
                {
                    Console.WriteLine("[RELATION] Wrong type in line {0}", line);
                    Kompilator.errors++;
                }
            }
            type = Typ.Bool;
        }

        public override void GenCode()
        {
            rel.GenCode();
            if (rel.is_asi)
            {
                Kompilator.EmitCode("ldloc " + rel.val);
            }
            adding.GenCode();
            if (adding.is_asi)
            {
                Kompilator.EmitCode("ldloc " + adding.val);
            }
            if (trel == Typ.Int && tadd == Typ.Double)
            {
                Kompilator.EmitCode("stloc niewidzialnetemp");
                Kompilator.EmitCode("conv.r8");
                Kompilator.EmitCode("ldloc niewidzialnetemp");
            }
            else if (trel == Typ.Double && tadd == Typ.Int)
            {
                Kompilator.EmitCode("conv.r8");
            }

            if (sig == "==")
                Kompilator.EmitCode("ceq");
            else if (sig == "!=")
            {
                Kompilator.EmitCode("ceq");
                Kompilator.EmitCode("ldc.i4.0");
                Kompilator.EmitCode("ceq");
            }
            else if (sig == ">")
                Kompilator.EmitCode("cgt");
            else if (sig == ">=")
            {
                Kompilator.EmitCode("clt");
                Kompilator.EmitCode("ldc.i4.0");
                Kompilator.EmitCode("ceq");
            }
            else if (sig == "<=")
            {
                Kompilator.EmitCode("cgt");
                Kompilator.EmitCode("ldc.i4.0");
                Kompilator.EmitCode("ceq");
            }
            else
                Kompilator.EmitCode("clt");
        }
    }
    public class Logic : SyntaxTree
    {


        public SyntaxTree logic;
        public SyntaxTree rel;
        public string sig;
        public Logic(SyntaxTree l, string s, SyntaxTree a, int li)
        {
            line = li;
            logic = l;
            sig = s;
            rel = a;
        }

        public override void CheckType()
        {
            logic.CheckType();
            rel.CheckType();

            if (logic.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(logic.val))
                {
                    Console.WriteLine("[LOGIC] undeclared variable line {0}", line);
                    Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[logic.val].typ != Typ.Bool)
                {
                    Console.Write("[LOGIC] wrong type in line {0}", line);
                    Kompilator.errors++;
                }
            }
            else if (logic.type != Typ.Bool)
            {
                Console.WriteLine("[LOGIC] Wrong type in line {0}", line);
                Kompilator.errors++;
            }
            if (rel.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(rel.val))
                {
                    Console.WriteLine("[LOGIC] undeclared variable line {0}", line);
                    Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[rel.val].typ != Typ.Bool)
                {
                    Console.Write("[LOGIC] wrong type in line {0}", line);
                    Kompilator.errors++;
                }
            }
            else if (rel.type != Typ.Bool)
            {
                Console.WriteLine("[LOGIC] Wrong type in line {0}", line);
                Kompilator.errors++;
            }
            type = Typ.Bool;
        }

        public override void GenCode()
        {
            if (sig == "and")
            {

                logic.GenCode();
                if (logic.is_asi)
                {
                    Kompilator.EmitCode("ldloc " + logic.val);
                }
                Kompilator.EmitCode("brfalse.s e{0}", Kompilator.nr_etykiety);

                rel.GenCode();
                if (rel.is_asi)
                {
                    Kompilator.EmitCode("ldloc " + rel.val);
                }
                if (rel.is_asi)
                {
                    Kompilator.EmitCode("ldloc " + rel.val);
                }
                Kompilator.EmitCode("br.s e{0}", Kompilator.nr_etykiety + 1);
                Kompilator.EmitCode("e{0}: ldc.i4.0", Kompilator.nr_etykiety);

                Kompilator.nr_etykiety++;
                Kompilator.EmitCode("e{0}: nop", Kompilator.nr_etykiety);
                Kompilator.nr_etykiety++;
            }
            else
            {
                logic.GenCode();
                if (logic.is_asi)
                {
                    Kompilator.EmitCode("ldloc " + logic.val);
                }
                Kompilator.EmitCode("brtrue.s e{0}", Kompilator.nr_etykiety);

                rel.GenCode();
                if (rel.is_asi)
                {
                    Kompilator.EmitCode("ldloc " + rel.val);
                }
                Kompilator.EmitCode("br.s e{0}", Kompilator.nr_etykiety + 1);
                Kompilator.EmitCode("e{0}: ldc.i4.1", Kompilator.nr_etykiety);

                Kompilator.nr_etykiety++;
                Kompilator.EmitCode("e{0}: nop", Kompilator.nr_etykiety);
                Kompilator.nr_etykiety++;
            }

        }
    }
    public class Add : SyntaxTree
    {


        public SyntaxTree adding;
        public SyntaxTree rel;
        public string sig;
        Typ tadd;
        Typ trel;
        public Add(SyntaxTree ad, string s, SyntaxTree r, int li)
        {
            line = li;
            adding = ad;
            sig = s;
            rel = r;
        }

        public override void CheckType()
        {
            adding.CheckType();

            rel.CheckType();
            
            if(adding.type==Typ.Iden)
            {
               if(!Kompilator.indetyfikatory.ContainsKey(adding.val))
                {
                    Console.WriteLine("[ADD/SUB/DIV/MUL] undeclare variable{0}", line);
                    Kompilator.errors++;
                }
                else if(Kompilator.indetyfikatory[adding.val].typ == Typ.Bool)
                {
                    Console.WriteLine("[ADD/SUB/DIV/MUL] wrong type line {0}", line);
                    Kompilator.errors++;
                }
               else
                {
                    tadd = Kompilator.indetyfikatory[adding.val].typ;
                }
               
            }
           else if (adding.type == Typ.Bool )
            {
                Console.WriteLine("[ADD/SUB/DIV/MUL] wrong type line {0}", line);
                Kompilator.errors++;
            }
            else
            {
                tadd = adding.type;
            }
            if (rel.type == Typ.Iden)
            {
                if (!Kompilator.indetyfikatory.ContainsKey(rel.val))
                {
                    Console.WriteLine("[ADD/SUB/DIV/MUL] undeclare variable{0}", line);
                    Kompilator.errors++;
                }
                else if (Kompilator.indetyfikatory[rel.val].typ == Typ.Bool)
                {
                    Console.WriteLine("[ADD/SUB/DIV/MUL] wrong type line {0}", line);
                    Kompilator.errors++;
                }
                else
                {
                    trel = Kompilator.indetyfikatory[rel.val].typ;
                }

            }
            else if (rel.type == Typ.Bool)
            {
                Console.WriteLine("[ADD/SUB/DIV/MUL] wrong type line {0}", line);
                Kompilator.errors++;
            }
            else
            {
                trel = rel.type;
            }            
           
            if (tadd == Typ.Double || trel == Typ.Double)
            {
                type = Typ.Double;
            }
            else
            {
                type = Typ.Int;
            }

        }

        public override void GenCode()
        {
            adding.GenCode();
            if (adding.is_asi)
            {
                Kompilator.EmitCode("ldloc " + adding.val);
            }
            rel.GenCode();
            if (rel.is_asi)
            {
                Kompilator.EmitCode("ldloc " + rel.val);
            }
            bool t1 = (tadd == Typ.Int && trel == Typ.Double);
            bool t2 = (tadd == Typ.Double && trel == Typ.Int);


            if (t1 || t2)
            {

                if (t1)
                {
                    Kompilator.EmitCode("stloc niewidzialnetemp");
                    Kompilator.EmitCode("conv.r8");
                    Kompilator.EmitCode("ldloc niewidzialnetemp");
                }
                else if (t2)
                {
                    Kompilator.EmitCode("conv.r8");

                }
            }

            if (sig == "+")
                Kompilator.EmitCode("add");
            else if (sig == "-")
                Kompilator.EmitCode("sub");
            else if (sig == "*")
                Kompilator.EmitCode("mul");
            else
            {
                //if (tadd == Typ.Int && trel== Typ.Int && type == Typ.Double)
                //{
                //    Kompilator.EmitCode("conv.r8");
                //    Kompilator.EmitCode("stloc niewidzialnetemp");
                //    Kompilator.EmitCode("conv.r8");
                //    Kompilator.EmitCode("ldloc niewidzialnetemp");

                //}
                Kompilator.EmitCode("div");

            }
        }
    }
    public class Return : SyntaxTree
    {

        public Return(int l)
        {
            line = l;
        }

        public override void CheckType()
        {

        }

        public override void GenCode()
        {
           // Kompilator.retu = true;
            Kompilator.is_ret = true;
            Kompilator.EmitCode("br.s ereturn");
        }
    }
    public class Read : SyntaxTree
    {
        public string Iden;
        public Read(string id, int l)
        {
            Iden = "z"+id;
            line = l;
        }

        public override void CheckType()
        {
            if (!Kompilator.indetyfikatory.ContainsKey(Iden))
            {
                Console.WriteLine("undeclared variable line {0}", line);
            }
        }

        public override void GenCode()
        {
            Kompilator.EmitCode("call string [mscorlib]System.Console::ReadLine()");
            if (Kompilator.indetyfikatory.ContainsKey(Iden) && Kompilator.indetyfikatory[Iden].typ == Typ.Int)
                Kompilator.EmitCode("call int32 [mscorlib] System.Int32::Parse(string)");
            else if (Kompilator.indetyfikatory.ContainsKey(Iden) && Kompilator.indetyfikatory[Iden].typ == Typ.Bool)
                Kompilator.EmitCode("call bool[mscorlib] System.Boolean::Parse(string)");
            else
                Kompilator.EmitCode("call float64 [mscorlib] System.Double::Parse(string)");
            Kompilator.EmitCode("stloc " + Iden);

        }
    }
    public class WriteExp : SyntaxTree
    {
        public SyntaxTree exp;
        public WriteExp(SyntaxTree e, int l)
        {
            exp = e;
            line = l;
        }

        public override void CheckType()
        {
            exp.CheckType();
            Typ typ = exp.type;
            if (typ == Typ.Iden && !Kompilator.indetyfikatory.ContainsKey(exp.val))
            {
                Console.WriteLine("undeclared variable line {0}", line);
                Kompilator.errors++;
            }
        }

        public override void GenCode()
        {
            exp.GenCode();
            if(exp.is_asi)
            {
                Kompilator.EmitCode("ldloc " + exp.val);
            }
            string t;
            t = "\"0.000000\"";
            Typ typ = exp.type;
            if ((typ == Typ.Iden && Kompilator.indetyfikatory[exp.val].typ == Typ.Double) || exp.type == Typ.Double)
            {
                if (typ == Typ.Iden)
                {

                    Kompilator.EmitCode("stloc niewidzialnetemp");
                    Kompilator.EmitCode("ldloca.s niewidzialnetemp");
                    Kompilator.EmitCode("ldstr  " + t);
                    Kompilator.EmitCode("call      class [mscorlib]        System.Globalization.CultureInfo[mscorlib] System.Globalization.CultureInfo::get_InvariantCulture()");
                    Kompilator.EmitCode("call      instance string[mscorlib] System.Double::ToString(string, class [mscorlib] System.IFormatProvider)");
                    Kompilator.EmitCode("call void [mscorlib]System.Console::Write(string)");
                    Kompilator.EmitCode("nop");
                }
                else
                {

                    // Kompilator.EmitCode(".locals init (float64 niewydocznetmp)");
                    Kompilator.EmitCode("stloc niewidzialnetemp");
                    Kompilator.EmitCode("ldloca niewidzialnetemp");
                    Kompilator.EmitCode("ldstr  " + t);
                    Kompilator.EmitCode("call      class [mscorlib]        System.Globalization.CultureInfo[mscorlib] System.Globalization.CultureInfo::get_InvariantCulture()");
                    // Kompilator.EmitCode("box[mscorlib] System.Double");
                    Kompilator.EmitCode("call      instance string[mscorlib] System.Double::ToString(string, class [mscorlib] System.IFormatProvider)");
                    Kompilator.EmitCode("call void [mscorlib]System.Console::Write(string)");
                    Kompilator.EmitCode("nop");

                }

            }
            else
            {
                if (typ == Typ.Iden)
                {
                    if (Kompilator.indetyfikatory[exp.val].typ == Typ.Int)
                    {
                        Kompilator.EmitCode("call       void [mscorlib]System.Console::Write(int32)");
                        Kompilator.EmitCode("nop");
                    }
                    else
                    {

                        // Kompilator.EmitCode("ldloca.s " + exp.val);
                        Kompilator.EmitCode("call       void [mscorlib]System.Console::Write(bool)");
                        Kompilator.EmitCode("nop");
                    }

                }
                else
                {
                    if (typ == Typ.Int)
                        Kompilator.EmitCode("call       void [mscorlib]System.Console::Write(int32)");
                    else
                    {
                        Kompilator.EmitCode("call       void [mscorlib]System.Console::Write(bool)");
                    }
                    Kompilator.EmitCode("nop");
                }


            }



        }
    }
    public class WriteNapis : SyntaxTree
    {
        public string napis;
        public WriteNapis(string n, int l)
        {
            napis = n;
            line = l;
        }

        public override void CheckType()
        {

        }

        public override void GenCode()
        {

            Kompilator.EmitCode("ldstr " + napis);
            Kompilator.EmitCode("call void [mscorlib]System.Console::Write(string)");
        }
    }
}
