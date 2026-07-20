# Identity

> A deterministic identity resolution engine for building auditable and reproducible master identity repositories.

## Overview

Identity is an experimental platform whose goal is to solve one of the hardest problems in information systems: determining when data originating from multiple heterogeneous sources actually represents the same real-world entity.

Unlike traditional Master Data Management (MDM) or duplicate detection solutions, Identity does not rely on confidence scores or opaque heuristics. Every decision is deterministic, reproducible, traceable and fully explainable.

The formal derivation engine now serves as a frozen foundation for independently organized application modules.

## Vision

Modern organizations maintain information about the same entities across dozens of independent systems.

Examples include:

- CRM
- ERP
- HR
- Active Directory
- Business applications
- External partners
- CSV / Excel imports
- Legacy databases

Identity aims to build a single, coherent identity repository capable of:

- detecting duplicate entities;
- merging information while preserving complete traceability;
- maintaining a complete history of identity evolution;
- explaining every merge and every split;
- reproducing every decision at any point in time.

## Core principles

Identity is built around several non-negotiable principles:

- **Deterministic** – the same inputs always produce the same outputs.
- **Explainable** – every decision can be justified.
- **Auditable** – every derivation is traceable.
- **Reproducible** – historical results remain reproducible.
- **Versioned** – every evolution is governed by explicit contracts.
- **Immutable** – historical states are preserved.

## Current status

The current implementation contains the deterministic derivation engine and its first file-oriented modules.

It already provides:

- deterministic derivation of identity states;
- normative identity register management;
- transition computation between identity states;
- complete audit generation;
- deterministic identity indexing;
- extensive conformance tests.

The repository is organized by domain:

- `src/InstallChecker.Identity*`: the frozen generic Identity foundation;
- `modules/scanner/`: file observation and SQLite snapshot production;
- `modules/duplicate-files/`: exact duplicate analysis, retention reports and reviewed plans;
- `apps/cli/`: command routing and composition only.

Module documentation: [Scanner](modules/scanner/README.md) and
[Duplicate Files](modules/duplicate-files/README.md).

This engine is intended to become the core of the future Identity platform.

## Long-term roadmap

The derivation engine will eventually support higher-level capabilities such as:

- duplicate detection;
- entity resolution;
- golden record generation;
- master data management (MDM);
- identity governance;
- merge/split workflows;
- synchronization between information systems;
- quality monitoring dashboards;
- APIs and user interfaces for identity management.

## Philosophy

Identity treats identity resolution as a formal derivation problem rather than a probabilistic prediction problem.

Instead of answering:

> "These two records are probably identical."

Identity aims to answer:

> "These two records represent the same entity because the identity repository contains this exact sequence of justified derivation acts."

Every conclusion must be explainable, reproducible and auditable.

---

*Identity is an ongoing research and engineering project exploring deterministic identity resolution and explainable master data management.*
