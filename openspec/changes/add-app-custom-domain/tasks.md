## 1. Release 1: domain and DNS

- [x] 1.1 Add failing Terraform test assertions: the Pages domain is `app.seekajobs.tech` on project `seeka`, and the DNS record is a proxied CNAME `app` pointing at the project subdomain in the looked-up zone; verify they fail
- [x] 1.2 Add the zone lookup, `cloudflare_pages_domain`, and `cloudflare_dns_record`; verify `terraform validate` and `terraform test` pass
- [x] 1.3 Update the README deployment notes and the infra README
- [x] 1.4 Release, then confirm `https://app.seekajobs.tech/` loads and `/api/health` returns 200

## 2. Release 2: canonical host

- [x] 2.1 Write failing tests for the redirect: the production pages.dev host gets a 301 to the same path and query on the branded host; the branded host, preview hosts, and localhost pass through
- [x] 2.2 Add the root Pages middleware; verify the edge tests and typecheck pass, and that the frontend deploy workflow runs the new test
- [ ] 2.3 Set `CLOUDFLARE_PAGES_URL=https://app.seekajobs.tech` in the `production` environment, release, and confirm the redirect and a verification link that starts with the branded host
- [x] 2.4 `openspec validate add-app-custom-domain --strict`
