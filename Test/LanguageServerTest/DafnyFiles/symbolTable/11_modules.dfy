//Module Definitions

module M1 {
    class M1_Class {
        constructor(){}
        method M1_Class_Method() {}
    }

    method M1_Method() {}
}

module M2 {
        class M2_Class {
        constructor(){}
        method M2_Class_Method() {}
    }

    method M2_Method() {}
}

//Global Module Definitions 

class GlobalClass {
    constructor(){}
    method Method() {}
}
method Global_Method() {}

//Tests

module ImportWithIdentifier { 
    import Mod = M1       //kaum zugreifbar. wir haben nur | weirdo: in topleveldecls sind die "eigenen" klassen dirn!! wtf. wtf. ||| einfach nach aliasmoduledecl filtern?? || o.TopLevleDecl[0] = 'Mod' | tok->Mod | Root -> M1 (type literalmoduledecl) | root.moduledef.tok sowie root.tok enthielte filename und ort der definition. könnten fidnDecl dann nutzen. | ich sehe keinen tok der aufs m1 hier zeigt.
    method test1() {
        var m1 := new Mod.M1_Class();  //rhs ist hier TypeRhs. Der Type enthält den Punkt: Mod.M1_Class. type.Name ist aber nur M1_Class. wenn wir also was "inherited modules" hätten, könnten wir einfach die auch durchsuchen. dann ginge das auch für openend imports. ähnlich der vererbung. 'wichtig': e.tok its das ganze, mti dem punkt. e.type.tok ist nur das recchte. in nem zweiten schritt evtl split('.') und nach modulen suchen... oder evtl auch nciht....     --> nimm doch einfach erstmal e.Path.ResolvedClass, dann miot getSymbolAtPos... scheiss auf das links vom punkt.
        m1.M1_Class_Method(); //das hier ist jetzt weiderum ein ExprDotName. die LHS ist aber ganz normal - lokale variable. wir dauch bereits gefunden (müsste). aber die rechte seite ist noch nicht gut. da hab ich ja im visit "definingClassName". da nicht tostring, sondern ".Name", weil sonst der punkt mitkommt. Find Decl müsste dann nicht nur nach klassen, sondern die importierten module müsste auch durchsuchen. klasse kann ja nur in modul sein. btw. hmm.. könnte noch schwierig werden. mal sehen. danach aber easy dann, der rest stimmt.                 |¢||||||-> Also auch hier ist resolved ein MemberSelectExpr. Lhs wird ja ansich da oben gehandhabt. vlt wär es fast besser den resolvedExpresion zu visitien. hat member, und obj. obj ist ja einfach name segment, wie lhs. aber mebmer wär dann eben die methode. und zwra direkt die decl, eben schon resoled... dann müsste ich das nicht machen...
        Mod.M1_Method();       //auch das ist exporDotName, wobei resolved Expr auch hier memberselectexpr ist. die LHS ist Mod. Wenn ich für Mod ein Symbol hinterlege, würde das funzen. könnte mane ig machen. vlt, vlt auch nciht. linke seite in resolved ist werid. Die rechte seite ist eben exprodotname. e.LHS.Type ist jetzt 'null'. Keinerlei informationen drin... hmm. es hätte: Lhs.ResolvedEcxpression.Decl.Pathj. Is zwar n Array, aber da wär der Token wo wir suchen ümssten. das ist sone list, evtl komsich. besser: resolvedexpression.decl.root.moduleDef wenn man willl. dannw ürde es ja dort suchen und früher oder später auch die default class durchsuchen und dodrt die methode finden? evtl?   .... blabla |¬||--> ach hier resolfed.member.tok holen, danach suchen, ferttig.
    }
}
module ImportWithName {
    import M1                        
    method test2() {
        var m1 := new M1.M1_Class();
        m1.M1_Class_Method();
        M1.M1_Method();
    }

}

module ImportOpened{ 
    import opened M2  //auch hier kaum infos ausser dass M2 unter topLeveldecls ist. [neben _default]  unter root.moduleDef finden wir da wieder info, wo es ist. müssten aber prüfen, dass es ein "AliasModuleDecl" ist, das andere ist ja die _defaultClassDecl.
    method test3() {
        var m2 := new M2_Class();   //type rhs. im prinzip, ja, einfach die inherited modules durchsuchen... könnte sogar klappen. scheiss auf das linke vom punkt.  e.Path würde auf "M2_Class" zeigen. ansonsten. dann köntne man direkt dort drin auch suchen. aber dazu müsste man halt wieder erst eine suche nach dem path durchführen.
        m2.M2_Class_Method();       //exproDotName. type ist nicht bekannt. e.ResolvedExpression ist "MemberselectExpression". Evtl den visiten? Ansonsten hat der ResolvedExpression halt ein "Member" und "Obj". "Obj" hätte dann wiederum Type, was wir nutzen könnten, um die klasse zu finden. wollend ie mich eig verarschen.
        M2_Method();                //das hier ist ein stinknormales name segment. wenn es alle imports durchsucht, und deren defaults, könnte es das sogar finden.
    }
}

module ImportMultiple{
    import M1
    import M2
    method test3() {
        var m1 := new M1.M1_Class();
        var m2 := new M2.M2_Class();
        m2.M2_Class_Method();
        M1.M1_Method();
    }
}

module UsingGlobals{
    method test3() {
        //var g := new GlobalClass();
        //g.Method();
        //Global_Method();   //not supported by language
    }
}