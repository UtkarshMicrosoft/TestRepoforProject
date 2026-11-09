# Database tests

Tests for constraints, functions, views and triggers (DB-C03, DB-C06, DB-C07). They run against a real PostgreSQL 18 database, never an in-memory provider. Example: two lease activations racing for the same unit, where exactly one must succeed (M3-04).
