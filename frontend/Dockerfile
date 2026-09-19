# Build stage
FROM node:22-alpine AS builder

WORKDIR /app

# Copy root and workspace package files
COPY package*.json ./
COPY frontend/package*.json ./frontend/

# Install dependencies deterministically
RUN npm ci

# Copy frontend source
COPY frontend ./frontend

WORKDIR /app/frontend

ARG NEXT_PUBLIC_API_URL=http://localhost:4000
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}
ENV NEXT_TELEMETRY_DISABLED=1

RUN npm run build

# Production stage
FROM node:22-alpine AS runner

WORKDIR /app/frontend

ENV NODE_ENV=production
ENV PORT=3000
ENV NEXT_TELEMETRY_DISABLED=1

COPY --chown=node:node --from=builder /app/node_modules /app/node_modules
COPY --chown=node:node --from=builder /app/frontend/package*.json ./
COPY --chown=node:node --from=builder /app/frontend/.next ./.next
COPY --chown=node:node --from=builder /app/frontend/public ./public

EXPOSE 3000

USER node

CMD ["npm", "start"]


