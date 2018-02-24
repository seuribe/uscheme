using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UScheme
{
    public abstract class Number : Exp
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

        public abstract bool IsInteger();

        static CSharpProcedure BinaryOperation<TResult>(Func<Number, Number, TResult> op) where TResult : Exp {
            return new CSharpProcedure(list => {
                StdLib.EnsureArity(list, 2);
                var first = list.First as Number;
                var second = list.Second as Number;
                return op(first, second);
            });
        }

        static readonly CSharpProcedure BinaryAdd = BinaryOperation(Add);
        static readonly CSharpProcedure BinarySub = BinaryOperation(Sub);
        static readonly CSharpProcedure BinaryMult = BinaryOperation(Mult);
        static readonly CSharpProcedure BinaryDiv = BinaryOperation(Div);

        public static readonly CSharpProcedure MULT = new CSharpProcedure(parameters => {
            return StdLib.FoldlBase(BinaryMult, parameters);
        });

        public static readonly CSharpProcedure DIV = new CSharpProcedure(parameters => {
            return StdLib.FoldlBase(BinaryDiv, parameters);
        });

        public static readonly CSharpProcedure ADD = new CSharpProcedure(parameters => {
            return StdLib.FoldlBase(BinaryAdd, parameters);
        });

        public static readonly CSharpProcedure SUB = new CSharpProcedure(parameters => {
            return StdLib.FoldlBase(BinarySub, parameters);
        });

        static Boolean PairwiseComparison(Func<Number, Number, bool> compFunc, bool expected, Cell list) {
            StdLib.EnsureArityMin(list, 2);

            var first = list.First as Number;
            foreach (var exp in list.Rest().Iterate()) {
                var second = exp as Number;
                if (compFunc(first, second) != expected)
                    return Boolean.FALSE;
                else
                    first = second;
            }

            return Boolean.TRUE;
        }

        public static readonly CSharpProcedure EQUALS = new CSharpProcedure(parameters => {
            return PairwiseComparison(NumberEquals, true, parameters);
        });

        public static readonly CSharpProcedure LESSTHAN = new CSharpProcedure(parameters => {
            return PairwiseComparison(NumberLessThan, true, parameters);
        });

        public static readonly CSharpProcedure LESSOREQUALTHAN = new CSharpProcedure(parameters => {
            return PairwiseComparison(NumberLessOrEqualThan, true, parameters);
        });

        public static readonly CSharpProcedure GREATERTHAN = new CSharpProcedure(parameters => {
            return PairwiseComparison(NumberLessOrEqualThan, false, parameters);
        });

        public static readonly CSharpProcedure GREATEROREQUALTHAN = new CSharpProcedure(parameters => {
            return PairwiseComparison(NumberLessThan, false, parameters);
        });

        public static bool TryParse(string token, out Number number) {
            number = null;
            if (int.TryParse(token, out int intValue))
                number = new IntegerNumber(intValue);
            else if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                number = new RealNumber(floatValue);

            return number != null;
        }
    }

    public class IntegerNumber : Number {
        public override double DoubleValue { get { return (double)value; } }
        public override float FloatValue { get { return (float)value; } }
        public override int IntValue { get { return value; } }

        public readonly int value;

        public IntegerNumber(int value) {
            this.value = value;
        }

        public override bool IsInteger() => true;

        public override bool Equals(Number n) => value == n.ToInteger().value;
        public override bool LessThan(Number n) => value < n.ToInteger().value;
        public override bool LessOrEqualThan(Number n) => value <= n.ToInteger().value;

        public override string ToString() => value.ToString();

        public override IntegerNumber ToInteger() => this;
        public override RealNumber ToReal() => new RealNumber((float)value);

        public override Number Add(Number b) => (b is RealNumber) ? b.Add(this) : new IntegerNumber(value + b.IntValue);
        public override Number Mult(Number b) => (b is RealNumber) ? b.Mult(this) : new IntegerNumber(value * b.IntValue);
        public override Number Div(Number b) => new RealNumber(FloatValue / b.FloatValue);

        public override Number Sub(Number b) {
            return (b is RealNumber) ?
                (Number)new RealNumber(FloatValue - b.FloatValue) :
                new IntegerNumber(value - (b as IntegerNumber).value);
        }

        public override Number Neg() => new IntegerNumber(-value);

        public override bool UEquals(Exp other) {
            return this == other ||
                ((other is IntegerNumber) && (value == (other as IntegerNumber).value));
        }
    }

    public class RealNumber : Number {
        public override double DoubleValue { get { return (double)value; } }
        public override float FloatValue { get { return value; } }
        public override int IntValue { get { return (int)value; } }

        public readonly float value;

        public RealNumber(float value) {
            this.value = value;
        }

        public override bool IsInteger() => (value % 1 == 0);

        public override string ToString() => value.ToString();
        public override bool Equals(Number n) => value == n.ToReal().value;
        public override bool LessThan(Number n) => value < n.ToReal().value;
        public override bool LessOrEqualThan(Number n) => value <= n.FloatValue;
        public override IntegerNumber ToInteger() => new IntegerNumber((int)value);
        public override RealNumber ToReal() => this;
        public override Number Add(Number b) => new RealNumber(value + b.ToReal().value);
        public override Number Sub(Number b) => new RealNumber(value - b.ToReal().value);
        public override Number Mult(Number b) => new RealNumber(value * b.ToReal().value);
        public override Number Div(Number b) => new RealNumber(value / b.ToReal().value);
        public override Number Neg() => new RealNumber(-value);

        public override bool UEquals(Exp other) {
            return this == other ||
                ((other is RealNumber) && (value == (other as RealNumber).value));
        }
    }
}