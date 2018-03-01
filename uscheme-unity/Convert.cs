using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UScheme.Unity {

    public static class Convert {

        public static float ToFloat(Number number) {
            return number.FloatValue;
        }

        public static int ToInt(Number number) {
            return number.IntValue;
        }

        public static Number ToNumber(float number) {
            return new RealNumber(number);
        }

        public static Number ToNumber(int number) {
            return new IntegerNumber(number);
        }

        public static Vector3 ToVector3(Vector vector) {
            return new Vector3(ToFloat(vector[0] as Number), ToFloat(vector[1] as Number), ToFloat(vector[2] as Number));
        }

        public static Vector ToVector(Vector3 vector) {
            return new Vector(ToNumber(vector.x), ToNumber(vector.y), ToNumber(vector.z));
        }

        public static Vector3 ToVector2(Vector vector) {
            return new Vector3(ToFloat(vector[0] as Number), ToFloat(vector[1] as Number));
        }

        public static Vector ToVector(Vector2 vector) {
            return new Vector(ToNumber(vector.x), ToNumber(vector.y));
        }

        public static List<T> ToList<ST, T>(Cell cell, Func<ST, T> convert) where ST: Exp {
            var result = new List<T>();
            foreach (ST exp in cell.Iterate())
                result.Add(convert(exp));
            return result;
        }

        public static Cell ToCell<T>(List<T> list, Func<T, Exp> convert) {
            return Cell.BuildList(list.Select(convert).ToList());
        }
    }
}
