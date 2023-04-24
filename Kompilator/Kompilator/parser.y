%namespace Kompilator

%union
{
public string  val;
public SyntaxTree tree;
public string oper;
public Typ typ;
public int line;
}
%token<tree> Program 
%token<oper> Assign Or And Pipe Equal NotEqual BitAnd
%token<oper> Greater GreaterEqual Less LessEqual
%token<oper> Plus Minus Multiplies Divides Tilde Not
%token<tree> If Else While
%token Read Write
%token OpenPar ClosePar BracketLeft BracketRight Semicolon

%token Endl Eof Error
%token Int Double Bool 

%token Ident IntNumber RealNumber Napis Komentarz2 True False Return

%type<typ> typ
%type<tree> block declars instrs  declar instr 
%type<tree> exp cond whileLoop inRead outWrite 
%type<tree> boolVar
%type<tree> bitexp term adding rel logic unar factor boolVar 

%type<tree> line 

%% 

start     : start line { ++lineno; }
          | line { ++lineno; }
          ;

line		: Program BracketLeft block BracketRight  { if($3!=null)
														root = new Program($3,$3.line);} 
			| Program BracketLeft block BracketRight Komentarz2  {root = new Program($3,$3.line);} 			
			| Program BracketLeft  BracketRight Komentarz2  	
			| Program BracketLeft  BracketRight 
			| Program Eof { Console.WriteLine("No bracket of program");
			Kompilator.errors++;}	
			| error {Console.WriteLine("Parser Declaration error in line {0} ",$1.line);
				Kompilator.errors++;				
				YYABORT;}
			
			;



block		: declars instrs {
				if($2==null && $1!=null)
					$$=new Block($1,$1.line);
				else if($1==null && $2!=null)
					$$=new Block($2.line,$2);
				else if($1!=null && $2!=null)
					$$=new Block($1,$2,$2.line);
				}		
			
			;
			

declars		: declars declar {$$ =new Declars($1,$2,$2.line);}
			| /*empty*/;

instrs		: /*empty*/
			| instrs instr {
				if($2==null && $1!=null)
					$$ =new Instrs($2.line,$2);
				else if($1==null && $2!=null)
					$$ =new Instrs($2,$2.line);
				else if($1!=null && $2!=null)
				$$ =new Instrs($1,$2,$2.line);}	
			;


declar		: typ Ident Semicolon {$$=new Declar($1,$2.val,$2.line);}
			| error {Console.WriteLine("Parser Declaration error in line {0} ",$1.line);
				Kompilator.errors++;				
				YYABORT;}
			;

typ			: Int {$$=Typ.Int;}
			| Double {$$ = Typ.Double;}
			| Bool {$$ =Typ.Bool;};

instr		: BracketLeft  BracketRight 
			| BracketLeft instrs BracketRight {$$ =$2;}
			| exp Semicolon {$$= $1;}
			| cond 
			| whileLoop 
			| inRead 
			| outWrite 
			| Return Semicolon {$$ = new Return($1.line);}
			| error {Console.WriteLine("Parser Instruction error in line {0} ",$1.line);
				Kompilator.errors++;
				
				YYABORT;}
			;
			
cond		: If OpenPar exp ClosePar instr {$$ = new CondIf($3,$5,$3.line);}
			| If OpenPar exp ClosePar instr Else instr {$$ = new CondIfElse($3,$5,$7,$3.line);};


whileLoop	: While OpenPar exp ClosePar instr {
						if($3!=null && $5!=null)
							$$ = new While($3,$5,$3.line);
						else if($3!=null)
							$$ = new While($3,$3.line);};

inRead		: Read Ident Semicolon {$$ =new Read($2.val,$2.line);};

outWrite	: Write exp Semicolon {$$ =new WriteExp($2,$2.line);}
			| Write Napis Semicolon {$$ =new WriteNapis($2.val,$2.line);}
			| Write OpenPar Napis ClosePar Semicolon {$$ =new WriteNapis($3.val,$3.line);};

exp			: OpenPar exp ClosePar {$$=$2;}
			| Ident Assign logic {$$ = new Assign($1.val,$3,$1.line);}
			| logic;

logic		: logic Or rel {$$ = new Logic($1,"or",$3,$1.line);}
			| logic And rel {$$ = new Logic($1,"and",$3,$1.line);}
			| rel;
			
rel			: rel Equal adding {$$ = new Relation($1,"==",$3,$1.line);}
			| rel NotEqual adding {$$ = new Relation($1,"!=",$3,$1.line);}
			| rel Greater adding {$$ = new Relation($1,">",$3,$1.line);}
			| rel GreaterEqual adding {$$ = new Relation($1,">=",$3,$1.line);}
			| rel Less adding {$$ = new Relation($1,"<",$3,$1.line);}
			| rel LessEqual adding {$$ = new Relation($1,"<=",$3,$1.line);}
			| adding
			;
adding		: adding Plus term
               { $$ = new Add($1,"+", $3,$1.line);}
			| adding Minus term
               { $$ = new Add($1,"-", $3,$1.line) ;}
			| term
			;

term		: term Multiplies bitexp
				   {$$ = new Add($1,"*",$3,$1.line);}
			| term Divides bitexp
				   { $$ = new Add($1,"/",$3,$1.line) ;}
			| bitexp
			;

bitexp		: bitexp Pipe unar {$$ = new Bit($1,"|",$3,$1.line);}
			| bitexp BitAnd unar {$$ = new Bit($1,"&",$3,$1.line);}
			| unar;

unar		: Minus unar {$$= new UnarMinus($2,$2.line);}
			| Tilde unar {$$= new UnarTilda($2,$2.line);}
			| Not unar {$$= new UnarNot($2,$2.line);}
			| OpenPar Int ClosePar unar {$$= new ToInt($4,$4.line);}
			| OpenPar Double ClosePar unar {$$= new ToDouble($4,$4.line);}
			| factor;

factor    : OpenPar exp ClosePar  { $$ = $2; ;}
          | IntNumber  {  $$ = new IntNumber($1.val,$1.line)     ;}
          | RealNumber  { $$ = new RealNumber($1.val,$1.line) ;}
          | Ident { $$ = new Ident($1.val,$1.line);}
		  | boolVar;


boolVar		: True {$$ = new Bool(true,$1.line);}
			| False {$$ = new Bool(false,$1.line);};

%%

int lineno=1;
public SyntaxTree root;
public Parser(Scanner scanner) : base(scanner)
{ 

}





