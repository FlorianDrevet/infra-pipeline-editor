interface ApiErrorItem {
  code?: string;
  description?: string;
}

interface ApiErrorResponse {
  detail?: string;
  title?: string;
  message?: string;
  errorMessage?: string;
  errors?: ApiErrorItem[] | Record<string, string[]>;
}

interface ErrorWithResponse {
  message?: string;
  response?: {
    data?: ApiErrorResponse | string;
  };
}

const TECHNICAL_KEY_VAULT_ERROR_CODES = new Set([
  'GitRepository.SecretRetrievalFailed',
  'GitRepository.SecretStorageFailed',
]);

export function extractLayoutRepositoriesApiErrorMessage(
  error: unknown,
  technicalErrorFallbackMessage?: string,
): string | null {
  const errorWithResponse = asErrorWithResponse(error);
  const responseMessage = extractResponseMessage(
    errorWithResponse?.response?.data,
    technicalErrorFallbackMessage,
  );
  if (responseMessage) {
    return responseMessage;
  }

  if (typeof errorWithResponse?.message === 'string' && errorWithResponse.message.trim()) {
    return errorWithResponse.message;
  }

  if (error instanceof Error && error.message.trim()) {
    return error.message;
  }

  return null;
}

function extractResponseMessage(
  data: ApiErrorResponse | string | undefined,
  technicalErrorFallbackMessage?: string,
): string | null {
  if (typeof data === 'string') {
    return data.trim() || null;
  }

  if (!data) {
    return null;
  }

  const firstErrorCode = extractFirstErrorCode(data.errors);
  if (
    technicalErrorFallbackMessage
    && firstErrorCode
    && TECHNICAL_KEY_VAULT_ERROR_CODES.has(firstErrorCode)
  ) {
    return technicalErrorFallbackMessage;
  }

  const firstErrorDescription = extractFirstErrorDescription(data.errors);
  if (firstErrorDescription) {
    return firstErrorDescription;
  }

  return firstNonEmptyString(data.errorMessage, data.detail, data.message, data.title);
}

function extractFirstErrorDescription(errors: ApiErrorResponse['errors']): string | null {
  if (Array.isArray(errors)) {
    const firstError = errors.find((error) => typeof error.description === 'string' && error.description.trim());
    return firstError?.description?.trim() || null;
  }

  if (!errors) {
    return null;
  }

  const firstValidationMessage = Object.values(errors)
    .flat()
    .find((message) => typeof message === 'string' && message.trim());

  return firstValidationMessage?.trim() || null;
}

function extractFirstErrorCode(errors: ApiErrorResponse['errors']): string | null {
  if (!Array.isArray(errors)) {
    return null;
  }

  const firstError = errors.find((error) => typeof error.code === 'string' && error.code.trim());
  return firstError?.code?.trim() || null;
}

function firstNonEmptyString(...values: Array<string | undefined>): string | null {
  for (const value of values) {
    if (typeof value === 'string' && value.trim()) {
      return value.trim();
    }
  }

  return null;
}

function asErrorWithResponse(error: unknown): ErrorWithResponse | null {
  if (typeof error !== 'object' || error === null) {
    return null;
  }

  return error as ErrorWithResponse;
}