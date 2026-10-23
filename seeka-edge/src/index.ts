import { Container } from '@cloudflare/containers'
import {
  containerEnvironment,
  routeToApi,
  type SeekaEdgeEnvironment,
} from './routing'

export class SeekaApiContainer extends Container<SeekaEdgeEnvironment> {
  defaultPort = 8080
  sleepAfter = '10m'
  enableInternet = true

  constructor(
    context: DurableObjectState<{}>,
    environment: SeekaEdgeEnvironment,
  ) {
    super(context, environment)
    this.envVars = containerEnvironment(environment)
  }
}

export default {
  fetch(request: Request, environment: SeekaEdgeEnvironment): Promise<Response> {
    return routeToApi(request, environment.API_CONTAINER)
  },
} satisfies ExportedHandler<SeekaEdgeEnvironment>
