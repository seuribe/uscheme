﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UScheme
{
    abstract class Number : Exp
    {
        public abstract double DoubleValue { get; }
        public abstract float FloatValue { get; }
        public abstract int IntValue { get; }

        public abstract bool Equals(Number n);
        public abstract bool LessThan(Number n);
        public abstract bool LessOrEqualThan(Number n);
        public abstract IntegerNumber ToInteger();
        public abstract RealNumber ToReal();

        public abstract Number Add(Number b);
        public abstract Number Sub(Number b);
        public abstract Number Mult(Number b);
        public abstract Number Div(Number b);
        public abstract Number Neg();
        public abstract bool UEquals(Exp b);

        public static Number Add(Number a, Number b) => a.Add(b);
        public static Number Sub(Number a, Number b) => a.Sub(b);
        public static Number Mult(Number a, Number b) => a.Mult(b);
        public static Number Div(Number a, Number b) => a.Div(b);
        public static bool NumberEquals(Number a, Number b) => a.UEquals(b);
        public static bool NumberLessThan(Number a, Number b) => a.LessThan(b);
        public static bool NumberLessOrEqualThan(Number a, Number b) => a.LessOrEqualThan(b);

        protected static bool IsInteger(float number) => number % 1 == 0;

        static Procedure BinaryOperation<TResult>(Func<Number, Number, TResult> op) where TResult : Exp {
            return new Procedure((UList list, Env env) => {
                StdLib.EnsureArity(list, 2);
                var first = env.Eval(list[0]) as Number;
                var second = env.Eval(list[1]) as Number;
                return op(first, second);
            });
        }

        static readonly Procedure BinaryAdd = BinaryOperation(Add);
        static readonly Procedure BinarySub = BinaryOperation(Sub);
        static readonly Procedure BinaryMult = BinaryOperation(Mult);
        static readonly Procedure BinaryDiv = BinaryOperation(Div);

        public static readonly Procedure MULT = new Procedure((UList parameters, Env env) => {
            return StdLib.FoldlBase(BinaryMult, parameters, env);
        });

        public static readonly Procedure ADD = new Procedure((UList parameters, Env env) => {
            return StdLib.FoldlBase(BinaryAdd, parameters, env);
        });

        public static readonly Procedure SUB = new Procedure((UList parameters, Env env) => {
            return StdLib.FoldlBase(BinarySub, parameters, env);
        });

        static Boolean PairwiseComparison(Func<Number, Number, bool> compFunc, bool expected, UList list, Env env) {
            StdLib.EnsureArityMin(list, 2);
            var first = UScheme.Eval(list[0], env) as Number;
            for (int i = 1 ; i < list.Count ; i++) {
                var second = UScheme.Eval(list[i], env) as Number;
                if (compFunc(first, second) != expected)
                    return Boolean.FALSE;
                else
                    first = second;
            }
            return Boolean.TRUE;
        }

        public static readonly Procedure EQUALS = new Procedure((UList parameters, Env env) => {
            return PairwiseComparison(NumberEquals, true, parameters, env);
        });

        public static readonly Procedure LESSTHAN = new Procedure((UList parameters, Env env) => {
            return PairwiseComparison(NumberLessThan, true, parameters, env);
        });

        public static readonly Procedure LESSOREQUALTHAN = new Procedure((UList parameters, Env env) => {
            return PairwiseComparison(NumberLessOrEqualThan, true, parameters, env);
        });

        public static readonly Procedure GREATERTHAN = new Procedure((UList parameters, Env env) => {
            return PairwiseComparison(NumberLessOrEqualThan, false, parameters, env);
        });

        public static readonly Procedure GREATEROREQUALTHAN = new Procedure((UList parameters, Env env) => {
            return PairwiseComparison(NumberLessThan, false, parameters, env);
        });
    }

    class IntegerNumber : Number {
        public override double DoubleValue { get { return (double)value; } }
        public override float FloatValue { get { return (float)value; } }
        public override int IntValue { get { return (int)value; } }

        public readonly int value;
        public IntegerNumber(int value) {
            this.value = value;
        }

        public override string ToString() {
            return value.ToString();
        }

        public override bool Equals(Number n) {
            return value == n.ToInteger().value;
        }

        public override bool LessThan(Number n) {
            return value < n.ToInteger().value;
        }

        public override bool LessOrEqualThan(Number n) {
            return value <= n.ToInteger().value;
        }

        public override IntegerNumber ToInteger() {
            return this;
        }

        public override RealNumber ToReal() {
            return new RealNumber((float)value);
        }

        public override Number Add(Number b) {
            if (b is RealNumber) {
                return new RealNumber((float)value).Add(b as RealNumber);
            } else {
                return new IntegerNumber(value + (b as IntegerNumber).value);
            }
        }

        public override Number Mult(Number b) {
            if (b is RealNumber) {
                return new RealNumber((float)value).Mult(b as RealNumber);
            } else {
                return new IntegerNumber(value * (b as IntegerNumber).value);
            }
        }

        public override Number Div(Number b) {
            if (b is IntegerNumber) {
                IntegerNumber other = b as IntegerNumber;
                int res = (value / other.value);
                if (res * other.value == value) {
                    return new IntegerNumber(res);
                }
            }
            return new RealNumber((float)value).Div(b.ToReal());
        }

        public override Number Sub(Number b) {
            if (b is RealNumber) {
                return new RealNumber((float)value).Sub(b as RealNumber);
            } else {
                return new IntegerNumber(value - (b as IntegerNumber).value);
            }
        }

        public override Number Neg() {
            return new IntegerNumber(-value);
        }

        public override bool UEquals(Exp other) {
            return this == other ||
                ((other is IntegerNumber) && (value == (other as IntegerNumber).value));
        }
    }

    class RealNumber : Number {
        public override double DoubleValue { get { return (double)value; } }
        public override float FloatValue { get { return (float)value; } }
        public override int IntValue { get { return (int)value; } }

        public readonly float value;
        public RealNumber(float value)
        {
            this.value = value;
        }
        public override string ToString()
        {
            return value.ToString();
        }
        public override bool Equals(Number n)
        {
            return value == n.ToReal().value;
        }
        public override bool LessThan(Number n)
        {
            return value < n.ToReal().value;
        }
        public override bool LessOrEqualThan(Number n)
        {
            return value <= n.ToReal().value;
        }
        public override IntegerNumber ToInteger()
        {
            return new IntegerNumber((int)value);
        }
        public override RealNumber ToReal()
        {
            return this;
        }
        public override Number Add(Number b)
        {
            return new RealNumber(value + b.ToReal().value);
        }
        public override Number Sub(Number b)
        {
            return new RealNumber(value - b.ToReal().value);
        }
        public override Number Mult(Number b) {
            return new RealNumber(value * b.ToReal().value);
        }
        public override Number Div(Number b) {
            return new RealNumber(value / b.ToReal().value);
        }
        public override Number Neg() {
            return new RealNumber(-value);
        }

        public override bool UEquals(Exp other) {
            return this == other ||
                ((other is RealNumber) && (value == (other as RealNumber).value));
        }

    }
}