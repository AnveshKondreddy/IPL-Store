using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Models
{
    public enum ErrorKind
    {
        Validation,
        NotFound,
        Conflict
    }

    public sealed class Result<T>
    {
        public T? Value { get; }
        public string? Error { get; }
        public ErrorKind? ErrorKind { get; }
        public bool IsSuccess => Error is null;

        private Result(T value) => Value = value;
        private Result(string error, ErrorKind kind) { Error = error; ErrorKind = kind; }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> NotFound(string error) => new(error, Models.ErrorKind.NotFound);
        public static Result<T> ValidationError(string error) => new(error, Models.ErrorKind.Validation);
        public static Result<T> Conflict(string error) => new(error, Models.ErrorKind.Conflict);
    }
}
