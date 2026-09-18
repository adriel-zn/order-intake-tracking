/** Shape of ASP.NET Core's ProblemDetails, returned on 400/404/409 responses. */
export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}
