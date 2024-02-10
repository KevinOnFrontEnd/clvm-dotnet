namespace CLVMDotNet.Tools.Stages.Stage2;

public class Defaults
{
    public static List<string> DefaultMacrosSrc => new List<string>
    {
        @"
        // we have to compile this externally, since it uses itself
        // (defmacro defmacro (name params body)
        //     (qq (list (unquote name) (mod (unquote params) (unquote body))))
        // )
        (q . (""defmacro""
           (c (q . ""list"")
              (c (f 1)
                 (c (c (q . ""mod"")
                       (c (f (r 1))
                          (c (f (r (r 1)))
                             (q . ()))))
                    (q . ()))))))
        ",
        @"
        // (defmacro list ARGS
        //     ((c (mod args
        //         (defun compile-list
        //                (args)
        //                (if args
        //                    (qq (c (unquote (f args))
        //                          (unquote (compile-list (r args)))))
        //                    ()))
        //             (compile-list args)
        //         )
        //         ARGS
        //     ))
        // )
        (q ""list""
            (a (q #a (q #a 2 (c 2 (c 3 (q))))
                     (c (q #a (i 5
                                 (q #c (q . 4)
                                       (c 9 (c (a 2 (c 2 (c 13 (q))))
                                               (q)))
                                 )
                                 (q 1))
                               1))
                1))
        ",
        @"
        (defmacro function (BODY)
            (qq (opt (com (q . (unquote BODY))
                     (qq (unquote (macros)))
                     (qq (unquote (symbols)))))))
        ",
        @"
        (defmacro if (A B C)
            (qq (a
                (i (unquote A)
                   (function (unquote B))
                   (function (unquote C)))
                @)))
        ",
        // / operator at the clvm layer is becoming deprecated and
        // will be implemented using divmod.
        @"
        (defmacro / (A B) (qq (f (divmod (unquote A) (unquote B)))))
        "
    };
    
    
    //DEFAULT_MACRO_LOOKUP
    //build_default_macro_lookup
    //default_macro_lookup
}