namespace CLVMDotNet;

using System;

/// <summary>
/// This class implements the CLVM Object protocol in the simplest possible way,
/// by just having an "atom" and a "pair" field
/// </summary>
public class CLVMObject
{
    public dynamic? Atom { get; set; }
    public Tuple<dynamic, dynamic>? Pair { get; set; }

    public CLVMObject(dynamic? v)
    {
        if (v is CLVMObject clvmObj)
        {
            //existing clvmobject
            Atom = clvmObj.Atom;
            Pair = clvmObj.Pair;
        }
        else if (v is (_, _))
        {
            //valid tuple
            Atom = null;
            Pair = v;
        }
        else
        {
            //HACK: invalid tuple length checking
            //python can store lists of arbitrary types. c# tuples are strongly typed, but because
            //dynamic is needed to handle the any type, generics is needed to check the length of the v.
            //to make sure if a tuple is used, it cannot have more than 2 items in it.
            var type = v?.GetType();
            var s = type?.GetGenericArguments();
            if (s.Length > 2)
            {
                throw new ArgumentException("tuples must be of size 2");
            }
            
            //v is an atom, which means it can be any type (string,byte,byte[],[],sexp etc)
            Pair = null;
            Atom = v;
        }
    }

    public CLVMObject()
    {
    }

    public byte[] AsAtom()
    {
        return Atom;
    }
}