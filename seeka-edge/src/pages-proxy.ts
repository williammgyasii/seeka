type WorkerService = {
  fetch(request: Request): Promise<Response>
}

export function proxyApiRequest(
  request: Request,
  service: WorkerService,
): Promise<Response> {
  return service.fetch(request)
}
