// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace URegister.Infrastructure.Constants
{
    /// <summary>
    /// Типове операции в одитния лог
    /// </summary>
    public enum AuditType
    {
        None = 0,
        Create,
        Read,
        Update,
        Delete
    }
    public enum TypeAuditTask
    {
        None = 0,
        Repository = 1,
        GrpcClient = 2
    }
}
