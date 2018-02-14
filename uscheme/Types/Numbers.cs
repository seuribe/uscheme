using System;
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

        public static readonly Procedure ADD = new Procedure((UList list, Env env) => {
            Number prev = UScheme.Eval(list[0], env) as Number;
            foreach (Exp e in list.Tail()) {
                prev = prev.Add(UScheme.Eval(e, env) as Number);
            }
            return prev;
        });
        public static readonly Procedure SUB = new Procedure((UList list, Env env) => {
            Number prev = UScheme.Eval(list[0], env) as Number;
            if (list.Count == 1) {
                return prev.Neg();
            }
            foreach (Exp e in list.Tail()) {
                prev = prev.Sub(UScheme.Eval(e, env) as Number);
            }
            return prev;
        });
        public static readonly Procedure EQUALS = new Procedure((UList list, Env env) => {
            Number first = UScheme.Eval(list[0], env) as Number;
            foreach (Exp e in list.Tail()) {
                Number next = UScheme.Eval(e, env) as Number;
                if (!first.Equals(next)) {
                    return Boolean.FALSE;
                }
            }
            return Boolean.TRUE;
        });
        public static readonly Procedure LESSTHAN = new Procedure((UList list, Env env) => {
            Number prev = UScheme.Eval(list[0], env) as Number;
            foreach (Exp e in list.Tail()) {
                Number next = UScheme.Eval(e, env) as Number;
                if (!prev.LessThan(next)) {
                    return Boolean.FALSE;
                }
                prev = next;
            }
            return Boolean.TRUE;
        });
        public static readonly Procedure LESSOREQUALTHAN = new Procedure((UList list, Env env) => {
            Number prev = UScheme.Eval(list[0], env) as Number;
            foreach (Exp e in list.Tail()) {
                Number next = UScheme.Eval(e, env) as Number;
                if (!prev.LessOrEqualThan(next)) {
                    return Boolean.FALSE;
                }
                prev = next;
            }
            return Boolean.TRUE;
        });
        public static readonly Procedure GREATERTHAN = new Procedure((UList list, Env env) => {
            Number prev = UScheme.Eval(list[0], env) as Number;
            foreach (Exp e in list.Tail()) {
                Number next = UScheme.Eval(e, env) as Number;
                if (prev.LessOrEqualThan(next)) {
                    return Boolean.FALSE;
                }
                prev = next;
            }
            return Boolean.TRUE;
        });
        public static readonly Procedure GREATEROREQUALTHAN = new Procedure((UList list, Env env) => {
            Number prev = UScheme.Eval(list[0], env) as Number;
            foreach (Exp e in list.Tail()) {
                Number next = UScheme.Eval(e, env) as Number;
                if (prev.LessThan(next)) {
                    return Boolean.FALSE;
                }
                prev = next;
            }
            return Boolean.TRUE;
        });
    }

    class IntegerNumber : Number
    {
        public override double DoubleValue { get { return (double)value; } }
        public override float FloatValue { get { return (float)value; } }
        public override int IntValue { get { return (int)value; } }

        public readonly int value;
        public IntegerNumber(int value)
        {
            this.value = value;
        }
        public override string ToString()
        {
            return value.ToString();
        }
        public override bool Equals(Number n)
        {
            return value == n.ToInteger().value;
        }
        public override bool LessThan(Number n)
        {
            return value < n.ToInteger().value;
        }
        public override bool LessOrEqualThan(Number n)
        {
            return value <= n.ToInteger().value;
        }
        public override IntegerNumber ToInteger()
        {
            return this;
        }
        public override RealNumber ToReal()
        {
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

    class RealNumber : Number
    {
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