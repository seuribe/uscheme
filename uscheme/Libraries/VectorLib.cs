﻿namespace UScheme {
    public class VectorLib : CoreLib {
        private static readonly Procedure MakeVector = new CSharpProcedure(parameters => {
            EnsureArityWithin(parameters, 1, 2);
            EnsureIs<IntegerNumber>(parameters.First);
            int len = (parameters.First as IntegerNumber).IntValue;
            Exp fill = (parameters.Length() == 2) ? new IntegerNumber(0) : parameters.Second;
            var value = new Exp[len];
            for (int i = 0 ; i < len ; i++)
                value[i] = fill;
            return new Vector(value);
        });

        private static readonly Procedure VectorLength = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 1);
            return new IntegerNumber((parameters.First as Vector).Length);
        });

        private static readonly Procedure VectorFromElements = new CSharpProcedure(parameters => {
            return Vector.FromCell(parameters as Cell);
        });

        private static readonly Procedure VectorRef = new CSharpProcedure(parameters => {
            var vector = (parameters.First as Vector);
            var index = (parameters.Second as IntegerNumber).IntValue;
            return vector[index];
        });

        public static void AddLibrary(Env env) {
            env.Bind("make-vector", MakeVector);
            env.Bind("vector-length", VectorLength);
            env.Bind("vector", VectorFromElements);
            env.Bind("vector-ref", VectorRef);
        }
    }
}