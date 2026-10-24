export const engineVersion = 'sf-v1';
export const operationNames = ['rotate90','rotate180','mirrorHorizontal','mirrorDiagonal','shiftRight','shiftDown','cycleColors','swapRedBlue'];
export const mappings = [];
for(let a=0;a<8;a++)for(let b=0;b<8;b++)if(b!==a)for(let c=0;c<8;c++)if(c!==a&&c!==b)for(let d=0;d<8;d++)if(d!==a&&d!==b&&d!==c)mappings.push(Object.freeze([a,b,c,d]));
Object.freeze(mappings);
export const allCandidateIds = Object.freeze(mappings.map((_,index)=>index));

/** Applies one sf-v1 transformation to a new row-major 3×3 board. */
export function operation(board,index) {
  const output=new Array(9);
  if(!Number.isInteger(index)||index<0||index>7)throw new RangeError('Unknown signal operation.');
  for(let r=0;r<3;r++)for(let c=0;c<3;c++){
    const value=board[r*3+c];
    switch(index){
      case 0:output[c*3+2-r]=value;break;
      case 1:output[(2-r)*3+2-c]=value;break;
      case 2:output[r*3+2-c]=value;break;
      case 3:output[c*3+r]=value;break;
      case 4:output[r*3+(c+1)%3]=value;break;
      case 5:output[(r+1)%3*3+c]=value;break;
      case 6:output[r*3+c]=value===0?0:value%3+1;break;
      case 7:output[r*3+c]=value===1?2:value===2?1:value;break;
    }
  }
  return output;
}
/** Executes A–D stamps left to right using an injective mapping. */
export function execute(input,program,mapping) {
  let output=input.slice();
  for(let index=0;index<program.length;index++)output=operation(output,mapping[program.charCodeAt(index)-65]);
  return output;
}
export function sameBoard(left,right){return left.length===right.length&&left.every((value,index)=>value===right[index]);}
/** Filters only from a previously valid candidate set; observed outputs remain exact evidence. */
export function consistentCandidates(observations,previous=allCandidateIds){
  return previous.filter(id=>observations.every(observation=>sameBoard(execute(observation.input,observation.program,mappings[id]),observation.output)));
}
/** Counts genuinely distinct held-out outcomes rather than guessing one rule from equivalent evidence. */
export function analyzeTarget(candidateIds,target){
  const boards=new Map();
  for(const id of candidateIds){const output=execute(target.input,target.program,mappings[id]);boards.set(output.join(''),output);}
  return {candidateCount:candidateIds.length,targetOutcomeCount:boards.size,outputs:[...boards.values()]};
}
/** An underdetermined answer is consistent when any surviving language produces it. */
export function finish(candidateIds,target,answer,mapping){
  const analysis=analyzeTarget(candidateIds,target),actualOutput=execute(target.input,target.program,mapping);
  const allowed=analysis.outputs.some(output=>sameBoard(output,answer));
  return {verdict:!allowed?'inconsistent':analysis.targetOutcomeCount===1?'deduced':'consistent',actualMatch:sameBoard(actualOutput,answer),candidateCount:analysis.candidateCount,targetOutcomeCount:analysis.targetOutcomeCount,actualOutput,mapping:mapping.slice()};
}
