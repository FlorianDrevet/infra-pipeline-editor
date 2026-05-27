import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import {
  DsAutocompleteComponent,
  DsAutocompleteOption,
  DsButtonComponent,
  DsButtonVariant,
  DsSelectComponent,
  DsTextFieldComponent,
} from '../ds';
import { ProjectService } from '../../services/project.service';
import { PROVIDER_OPTIONS } from '../../utils/repository-dialog.utils';
import {
  RepositoryContentKind,
  VerifiedGitBranchResponse,
  VerifyGitConnectionRequest,
} from '../../interfaces/project-repository.interface';
import {
  RepositoryConnectionResult,
  RepositoryDraft,
  RepositorySlotState,
} from '../../../features/projects/create-project-wizard/create-project-wizard.types';

type VerificationState = 'idle' | 'checking' | 'verified' | 'failed';

/**
 * Inline form that manages git repository connection verification for a single repo slot.
 * Displays: providerType → repositoryUrl → personalAccessToken → "Verify" button → defaultBranch autocomplete.
 */
@Component({
  selector: 'app-repository-connection-form',
  standalone: true,
  imports: [
    FormsModule,
    TranslateModule,
    DsSelectComponent,
    DsTextFieldComponent,
    DsButtonComponent,
    DsAutocompleteComponent,
  ],
  templateUrl: './repository-connection-form.component.html',
  styleUrl: './repository-connection-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RepositoryConnectionFormComponent implements OnInit {
  private readonly projectService = inject(ProjectService);

  public readonly lockedKinds = input<RepositoryContentKind[]>([]);
  public readonly initialData = input<RepositoryDraft | undefined>(undefined);

  public readonly connectionVerified = output<RepositoryConnectionResult>();
  public readonly stateChanged = output<RepositorySlotState>();

  protected readonly providerOptions = PROVIDER_OPTIONS;
  protected readonly providerType = signal<string>('AzureDevOps');
  protected readonly repositoryUrl = signal<string>('');
  protected readonly personalAccessToken = signal<string>('');
  protected readonly defaultBranch = signal<string>('');
  protected readonly branchQuery = signal<string>('');

  protected readonly verificationState = signal<VerificationState>('idle');
  protected readonly verifiedBranches = signal<VerifiedGitBranchResponse[]>([]);

  protected readonly branchOptions = computed<ReadonlyArray<DsAutocompleteOption<string>>>(() => {
    const query = this.branchQuery().trim().toLocaleLowerCase();
    return this.verifiedBranches()
      .filter((branch) => !query || branch.name.toLocaleLowerCase().includes(query))
      .map((branch) => ({
        value: branch.name,
        label: branch.name,
        icon: branch.isProtected ? 'verified_user' : 'lan',
      }));
  });

  protected readonly canVerify = computed(() => {
    return (
      this.providerType().trim().length > 0 &&
      this.repositoryUrl().trim().length > 0 &&
      this.personalAccessToken().trim().length > 0 &&
      this.verificationState() !== 'checking'
    );
  });

  protected readonly isBranchDisabled = computed(() => this.verificationState() !== 'verified');

  protected readonly verifyButtonVariant = computed<DsButtonVariant>(() => {
    switch (this.verificationState()) {
      case 'verified':
        return 'success';
      case 'failed':
        return 'danger';
      default:
        return 'secondary';
    }
  });

  protected readonly verifyButtonIcon = computed(() => {
    switch (this.verificationState()) {
      case 'verified':
        return 'check_circle';
      case 'failed':
        return 'error';
      default:
        return 'verified';
    }
  });

  protected readonly verificationStatusKey = computed(() => {
    switch (this.verificationState()) {
      case 'checking':
        return 'PROJECT_CREATE.STEP.LAYOUT.VERIFICATION_CHECKING';
      case 'verified':
        return 'PROJECT_CREATE.STEP.LAYOUT.VERIFICATION_SUCCESS';
      case 'failed':
        return 'PROJECT_CREATE.STEP.LAYOUT.VERIFICATION_FAILED';
      default:
        return '';
    }
  });

  public constructor() {
    effect(() => {
      const state = this.verificationState();
      const branch = this.defaultBranch();
      const isValid =
        state === 'verified' && this.isVerifiedBranch(branch);

      const data: RepositoryConnectionResult | null = isValid
        ? {
            providerType: this.providerType(),
            repositoryUrl: this.repositoryUrl(),
            defaultBranch: branch,
            personalAccessToken: this.personalAccessToken(),
          }
        : null;

      this.stateChanged.emit({ isValid, data });
    });
  }

  public ngOnInit(): void {
    const initial = this.initialData();
    if (initial) {
      this.providerType.set(initial.providerType || 'AzureDevOps');
      this.repositoryUrl.set(initial.repositoryUrl);
      this.defaultBranch.set(initial.defaultBranch);
    }
  }

  protected onProviderChange(value: string | number | null): void {
    this.providerType.set(String(value ?? ''));
    this.resetVerification();
  }

  protected onUrlChange(value: string): void {
    this.repositoryUrl.set(value);
    this.resetVerification();
  }

  protected onPatChange(value: string): void {
    this.personalAccessToken.set(value);
    this.resetVerification();
  }

  protected async verifyConnection(): Promise<void> {
    if (!this.canVerify()) {
      return;
    }

    this.verificationState.set('checking');

    const request: VerifyGitConnectionRequest = {
      providerType: this.providerType(),
      repositoryUrl: this.repositoryUrl().trim(),
      personalAccessToken: this.personalAccessToken().trim(),
    };

    try {
      const result = await this.projectService.verifyGitConnection(request);
      this.verifiedBranches.set(result.branches);

      const branch = result.defaultBranchCandidate ?? result.branches[0]?.name ?? '';
      this.defaultBranch.set(branch);
      this.branchQuery.set(branch);
      this.verificationState.set('verified');

      this.connectionVerified.emit({
        providerType: this.providerType(),
        repositoryUrl: this.repositoryUrl().trim(),
        defaultBranch: branch,
        personalAccessToken: this.personalAccessToken().trim(),
      });
    } catch {
      this.verifiedBranches.set([]);
      this.defaultBranch.set('');
      this.branchQuery.set('');
      this.verificationState.set('failed');
    }
  }

  protected onBranchSelected(option: DsAutocompleteOption<unknown>): void {
    const branchName = String(option.value);
    this.defaultBranch.set(branchName);
    this.branchQuery.set(branchName);
  }

  protected onBranchSearchChanged(query: string): void {
    this.branchQuery.set(query);
  }

  private resetVerification(): void {
    if (this.verificationState() === 'checking') {
      return;
    }
    this.verificationState.set('idle');
    this.verifiedBranches.set([]);
    this.defaultBranch.set('');
    this.branchQuery.set('');
  }

  private isVerifiedBranch(branchName: string): boolean {
    const normalized = branchName.trim();
    return this.verifiedBranches().some((branch) => branch.name === normalized);
  }
}
